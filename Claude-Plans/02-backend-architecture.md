# Backend Architecture — ASP.NET Core 9
> Cập nhật: 2026-06-18
> Mục tiêu: đơn giản nhất có thể để ra được sản phẩm trong 7 ngày.
> Core product: 1 video/ảnh → 5 MP4 variations với hook khác nhau.

---

## Cấu Trúc Solution

```
GenerateAIVideoStudio.sln
│
└── src/
    ├── Api/              ← ASP.NET Core Web API + Razor Pages
    ├── Application/      ← Business logic, Commands, Queries, Interfaces
    ├── Domain/           ← Entities, Constants
    └── Infrastructure/   ← EF Core, AI APIs, Storage, FFmpeg
```

> Hangfire chạy trong cùng process với Api — `WorkerCount=1` cho MVP (tránh FFmpeg CPU spike).

---

## Domain Layer

```
src/Domain/
├── Entities/
│   ├── VideoJob.cs
│   ├── JobBrief.cs        ← hooks_json, ctas_json, voice_script, voice_url
│   ├── JobVariation.cs    ← variation_index, hook_text, output_url
│   └── ApiCost.cs
└── Constants/
    ├── InputType.cs       ← Video | Images
    ├── JobStatus.cs       ← Pending | Scripting | Voicing | Composing | Generating | Done | Error
    └── AiService.cs       ← openai | elevenlabs | fptai | r2
```

---

## Application Layer

```
src/Application/
├── Common/
│   └── Interfaces/
│       ├── IVideoJobRepository.cs
│       ├── IJobBriefRepository.cs
│       ├── IContentIntelligenceService.cs   ← CORE: GPT sinh hooks/CTAs/script
│       ├── IVoiceSynthesisService.cs
│       ├── IVideoComposerService.cs         ← ComposeBaseAsync + ComposeVariationAsync
│       └── IStorageService.cs
│
├── Jobs/
│   ├── Commands/
│   │   ├── CreateJob/
│   │   │   ├── CreateJobCommand.cs
│   │   │   └── CreateJobCommandHandler.cs
│   │   └── ProcessJobFlowD/               ← PIPELINE CHÍNH
│   │       ├── ProcessFlowDCommand.cs
│   │       └── ProcessFlowDCommandHandler.cs
│   └── Queries/
│       ├── GetJob/
│       │   ├── GetJobQuery.cs
│       │   └── GetJobQueryHandler.cs
│       └── GetJobList/
│           ├── GetJobListQuery.cs
│           └── GetJobListQueryHandler.cs
│
└── DependencyInjection.cs
```

---

## Interfaces

### IContentIntelligenceService — CORE

```csharp
// src/Application/Common/Interfaces/IContentIntelligenceService.cs
public interface IContentIntelligenceService
{
    // GPT phân tích sản phẩm → sinh hooks, CTAs, voiceScript
    // Không hard-code hook types — GPT tự chọn angle phù hợp nhất
    Task<ContentBrief> GenerateBriefAsync(
        string productDescription,
        string inputType,               // "video" | "images"
        CancellationToken ct = default);
}

public record ContentBrief(
    string[] Hooks,      // 5 hooks — GPT tự chọn angle (pain/desire/curiosity/proof/comparison...)
    string[] Ctas,       // 3 CTAs
    string VoiceScript   // 30-45s voiceover script
);
```

### IVideoComposerService

```csharp
public interface IVideoComposerService
{
    // Bước 3: tạo base video (color grade + voiceover + caption + music)
    Task<string> ComposeBaseAsync(
        string inputObjectKey,      // R2 key của raw video hoặc ảnh đầu tiên
        string inputType,           // "video" | "images"
        string voiceoverObjectKey,  // R2 key của .mp3
        string[]? imageObjectKeys = null,  // thêm ảnh nếu inputType = "images"
        CancellationToken ct = default);

    // Bước 4: tạo 1 variation từ base video (thêm hook text overlay đầu video)
    Task<string> ComposeVariationAsync(
        string baseVideoObjectKey,
        string hookText,
        string ctaText,
        int variationIndex,
        CancellationToken ct = default);
}
```

### Interfaces khác

```csharp
public interface IVoiceSynthesisService
{
    Task<string> SynthesizeAsync(
        string text, string voiceId, CancellationToken ct = default);
    Task<List<VoiceOption>> GetAvailableVoicesAsync(
        string language = "vi", CancellationToken ct = default);
}

public record VoiceOption(string Id, string Name, string Accent, string Language);

public interface IStorageService
{
    Task<string> UploadAsync(Stream stream, string objectKey, string contentType, CancellationToken ct = default);
    Task<string> GetPresignedUrlAsync(string objectKey, int expiryHours = 168, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string objectKey, CancellationToken ct = default);
    Task DeleteAsync(string objectKey, CancellationToken ct = default);
}

public interface IVideoJobRepository
{
    Task<VideoJob?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<VideoJob>> GetListAsync(string? status, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(VideoJob job, CancellationToken ct = default);
    Task UpdateAsync(VideoJob job, CancellationToken ct = default);
}

public interface IJobBriefRepository
{
    Task AddAsync(JobBrief brief, CancellationToken ct = default);
    Task UpdateAsync(JobBrief brief, CancellationToken ct = default);
}
```

---

## ProcessFlowDCommandHandler — Pipeline 4 Bước

```csharp
public class ProcessFlowDCommandHandler : IRequestHandler<ProcessFlowDCommand>
{
    private readonly IVideoJobRepository _jobRepo;
    private readonly IJobBriefRepository _briefRepo;
    private readonly IContentIntelligenceService _contentIntelligence;
    private readonly IVoiceSynthesisService _voiceService;
    private readonly IVideoComposerService _videoComposer;
    private readonly IStorageService _storage;
    private readonly AppDbContext _db;

    public async Task Handle(ProcessFlowDCommand request, CancellationToken ct)
    {
        var job = await _jobRepo.GetByIdAsync(request.JobId, ct)
            ?? throw new InvalidOperationException($"Job {request.JobId} not found");

        try
        {
            // STEP 1 — Content Intelligence
            await SetStatus(job, JobStatus.Scripting, ct);
            var brief = await _contentIntelligence.GenerateBriefAsync(
                job.ProductDescription ?? "sản phẩm",
                job.InputType, ct);

            var jobBrief = new JobBrief
            {
                JobId       = job.Id,
                HooksJson   = JsonSerializer.SerializeToDocument(brief.Hooks),
                CtasJson    = JsonSerializer.SerializeToDocument(brief.Ctas),
                VoiceScript = brief.VoiceScript
            };
            await _briefRepo.AddAsync(jobBrief, ct);
            await TrackCost(job.Id, AiService.OpenAi, "content_brief", 0.015m, ct);

            // STEP 2 — Voice Synthesis
            await SetStatus(job, JobStatus.Voicing, ct);
            var voiceKey = await _voiceService.SynthesizeAsync(
                brief.VoiceScript, request.VoiceId ?? "default-vi", ct);
            jobBrief.VoiceUrl = voiceKey;
            await _briefRepo.UpdateAsync(jobBrief, ct);
            await TrackCost(job.Id, AiService.ElevenLabs, "tts", 0.030m, ct);

            // STEP 3 — Base Composition (FFmpeg)
            await SetStatus(job, JobStatus.Composing, ct);
            var baseVideoKey = await _videoComposer.ComposeBaseAsync(
                inputObjectKey    : job.InputUrl!,
                inputType         : job.InputType,
                voiceoverObjectKey: voiceKey,
                ct: ct);

            // STEP 4 — Variation Loop (chạy song song)
            await SetStatus(job, JobStatus.Generating, ct);
            var primaryCta = brief.Ctas.FirstOrDefault() ?? "Xem ngay!";

            var variationTasks = brief.Hooks.Select(async (hook, i) =>
            {
                var variationKey = await _videoComposer.ComposeVariationAsync(
                    baseVideoObjectKey: baseVideoKey,
                    hookText          : hook,
                    ctaText           : primaryCta,
                    variationIndex    : i,
                    ct                : ct);

                return new JobVariation
                {
                    JobId          = job.Id,
                    VariationIndex = i,
                    HookText       = hook,
                    CtaText        = primaryCta,
                    OutputUrl      = variationKey,
                    Status         = JobStatus.Done
                };
            });

            var variations = await Task.WhenAll(variationTasks);
            foreach (var variation in variations)
                await _db.JobVariations.AddAsync(variation, ct);
            await _db.SaveChangesAsync(ct);

            job.Status      = JobStatus.Done;
            job.CompletedAt = DateTime.UtcNow;
            await _jobRepo.UpdateAsync(job, ct);
        }
        catch (Exception ex)
        {
            job.Status       = JobStatus.Error;
            job.ErrorMessage = ex.Message;
            await _jobRepo.UpdateAsync(job, ct);
            throw;
        }
    }

    private async Task SetStatus(VideoJob job, string status, CancellationToken ct)
    {
        job.Status = status;
        await _jobRepo.UpdateAsync(job, ct);
    }

    private async Task TrackCost(Guid jobId, string service, string action, decimal costUsd, CancellationToken ct)
    {
        await _db.ApiCosts.AddAsync(new ApiCost
        {
            JobId    = jobId,
            Service  = service,
            Action   = action,
            CostUsd  = costUsd
        }, ct);
        await _db.SaveChangesAsync(ct);
    }
}
```

---

## CreateJobCommandHandler

```csharp
public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, Guid>
{
    public async Task<Guid> Handle(CreateJobCommand request, CancellationToken ct)
    {
        var job = new VideoJob
        {
            ProductDescription = request.ProductDescription,
            InputType          = request.InputType ?? InputType.Video,
            InputUrl           = request.InputObjectKey,
            CustomerEmail      = request.CustomerEmail,
            Notes              = request.Notes
        };

        await _jobRepo.AddAsync(job, ct);
        return job.Id;
        // JobsController sẽ enqueue ProcessFlowDCommand vào Hangfire sau khi tạo
    }
}

public record CreateJobCommand(
    string? ProductDescription,
    string? InputType,            // "video" | "images"
    string? InputObjectKey,       // R2 object key sau khi upload
    string? VoiceId,
    string? CustomerEmail,
    string? Notes
) : IRequest<Guid>;
```

---

## Controllers

### UploadController

```csharp
[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(200_000_000)] // 200MB
    public async Task<IActionResult> Upload(
        IFormFile file, CancellationToken ct)
    {
        var ext       = Path.GetExtension(file.FileName).ToLower();
        var inputType = ext is ".mp4" or ".mov" or ".avi" ? InputType.Video : InputType.Images;
        var objectKey = $"inputs/{Guid.NewGuid()}{ext}";

        await using var stream = file.OpenReadStream();
        await _storage.UploadAsync(stream, objectKey, file.ContentType, ct);

        return Ok(new { objectKey, inputType });
    }
}
```

### JobsController

```csharp
[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateJobCommand cmd, CancellationToken ct)
    {
        var jobId = await _mediator.Send(cmd, ct);

        // Enqueue pipeline vào Hangfire
        _hangfire.Enqueue<IMediator>(m =>
            m.Send(new ProcessFlowDCommand(jobId, cmd.VoiceId),
                   CancellationToken.None));

        return Accepted(new { jobId, status = JobStatus.Pending });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetJobQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? status, [FromQuery] int page = 1, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetJobListQuery(status, page), ct);
        return Ok(result);
    }
}
```

---

## Infrastructure Layer

```
src/Infrastructure/
├── Persistence/
│   ├── AppDbContext.cs
│   ├── Configurations/
│   │   ├── VideoJobConfiguration.cs
│   │   ├── JobBriefConfiguration.cs
│   │   ├── JobVariationConfiguration.cs
│   │   └── ApiCostConfiguration.cs
│   └── Repositories/
│       ├── VideoJobRepository.cs
│       └── JobBriefRepository.cs
│
├── AI/
│   ├── OpenAiContentIntelligenceService.cs   ← IContentIntelligenceService
│   ├── ElevenLabsVoiceSynthesisService.cs    ← IVoiceSynthesisService (dùng nếu quality ok)
│   └── FptAiVoiceSynthesisService.cs         ← IVoiceSynthesisService (nếu ElevenLabs kém)
│
│   (Giữ lại nhưng không build thêm:)
│   ├── HeyGenAvatarVideoService.cs           ← deprioritized đến V6
│   ├── KlingService.cs                       ← deprioritized đến V6
│   └── ShopeeProductScraper.cs               ← deprioritized đến V4+
│
├── Storage/
│   └── CloudflareR2StorageService.cs
│
├── Video/
│   └── FfmpegVideoComposerService.cs         ← ComposeBaseAsync + ComposeVariationAsync
│
└── DependencyInjection.cs
```

---

## DependencyInjection.cs

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, IConfiguration config)
{
    // Database
    services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(config.GetConnectionString("DefaultConnection")));

    // Repositories
    services.AddScoped<IVideoJobRepository, VideoJobRepository>();
    services.AddScoped<IJobBriefRepository, JobBriefRepository>();

    // AI Services
    services.AddScoped<IContentIntelligenceService, OpenAiContentIntelligenceService>();

    // Voice — đổi implementation dựa trên kết quả test Ngày 0
    // Nếu FPT AI tốt hơn:
    services.AddHttpClient<IVoiceSynthesisService, FptAiVoiceSynthesisService>();
    // Nếu ElevenLabs tốt hơn:
    // services.AddHttpClient<IVoiceSynthesisService, ElevenLabsVoiceSynthesisService>();

    // Storage + Video
    services.AddSingleton<IStorageService, CloudflareR2StorageService>();
    services.AddScoped<IVideoComposerService, FfmpegVideoComposerService>();

    return services;
}
```

---

## EF Core Configurations (ví dụ)

```csharp
public class JobBriefConfiguration : IEntityTypeConfiguration<JobBrief>
{
    public void Configure(EntityTypeBuilder<JobBrief> builder)
    {
        builder.ToTable("job_briefs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.HooksJson).HasColumnType("jsonb");
        builder.Property(x => x.CtasJson).HasColumnType("jsonb");
        builder.HasOne(x => x.Job)
               .WithOne(x => x.Brief)
               .HasForeignKey<JobBrief>(x => x.JobId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class JobVariationConfiguration : IEntityTypeConfiguration<JobVariation>
{
    public void Configure(EntityTypeBuilder<JobVariation> builder)
    {
        builder.ToTable("job_variations");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Job)
               .WithMany(x => x.Variations)
               .HasForeignKey(x => x.JobId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.JobId);
        builder.HasIndex(x => new { x.JobId, x.VariationIndex }).IsUnique();
    }
}
```

---

## Packages Cần Cài

```bash
# Application
dotnet add package MediatR --version 12.4.1
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.11.0

# Infrastructure
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.4
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.4
dotnet add package OpenAI --version 2.2.0
dotnet add package AWSSDK.S3 --version 3.7.400

# Api
dotnet add package Hangfire.AspNetCore --version 1.8.14
dotnet add package Hangfire.PostgreSql --version 1.20.9
dotnet add package Swashbuckle.AspNetCore --version 7.2.0
```
