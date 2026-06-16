namespace Application.Jobs.Commands.ProcessJobFlowA;

using Application.Common.Interfaces;
using Application.Common.DTOs;
using Domain.Entities;
using Domain.Constants;
using MediatR;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class ProcessFlowACommandHandler : IRequestHandler<ProcessFlowACommand>
{
    private readonly IVideoJobRepository _jobRepo;
    private readonly IScriptGeneratorService _scriptGen;
    private readonly IVoiceSynthesisService _voiceService;
    private readonly IAvatarVideoService _avatarService;
    private readonly IVideoComposerService _composer;
    private readonly IStorageService _storage;

    public ProcessFlowACommandHandler(
        IVideoJobRepository jobRepo,
        IScriptGeneratorService scriptGen,
        IVoiceSynthesisService voiceService,
        IAvatarVideoService avatarService,
        IVideoComposerService composer,
        IStorageService storage)
    {
        _jobRepo = jobRepo;
        _scriptGen = scriptGen;
        _voiceService = voiceService;
        _avatarService = avatarService;
        _composer = composer;
        _storage = storage;
    }

    public async Task Handle(ProcessFlowACommand request, CancellationToken ct)
    {
        var job = await _jobRepo.GetByIdAsync(request.JobId, ct);
        if (job == null)
            throw new ArgumentException($"Job with id {request.JobId} not found");

        var product = JsonSerializer.Deserialize<ProductInfo>(job.ProductInfo!.RootElement.GetRawText())!;

        try
        {
            // 1. Generate script
            await SetStatus(job, JobStatus.Scripting, ct);
            var script = await _scriptGen.GenerateAsync(product, job.VideoType!, job.TargetAudience!, ct: ct);
            job.Script = JsonSerializer.SerializeToDocument(script);
            await _jobRepo.UpdateAsync(job, ct);
            await Track(job, AiService.OpenAi, "script", 0.005m, ct);

            // 2. Synthesize voice
            await SetStatus(job, JobStatus.Voicing, ct);
            var variation = new JobVariation
            {
                JobId = job.Id,
                HookText = script.Hook,
                VoiceId = request.VoiceId ?? "default-vi",
                AvatarId = request.AvatarId ?? "default",
                VariationIndex = 0
            };
            var audioKey = await _voiceService.SynthesizeAsync(script.FullText, variation.VoiceId, ct);
            variation.AudioUrl = audioKey;
            await Track(job, AiService.ElevenLabs, "tts", 0.03m, ct);

            // 3. Render avatar video (HeyGen)
            await SetStatus(job, JobStatus.Rendering, ct);
            var heyGenJobId = await _avatarService.SubmitAsync(audioKey, variation.AvatarId, ct);
            
            // Poll HeyGen until done
            var heyGenResult = await PollUntilDone(
                () => _avatarService.GetResultAsync(heyGenJobId, ct),
                timeoutMin: 5, ct: ct);
            variation.AvatarVideoUrl = heyGenResult;
            await Track(job, AiService.HeyGen, "avatar_video", 0.45m, ct);

            // 4. FFmpeg compose
            await SetStatus(job, JobStatus.Composing, ct);
            var finalKey = await _composer.ComposeAsync(new VideoComposeRequest(
                AvatarVideoObjectKey: variation.AvatarVideoUrl,
                TryOnImageUrls: null,
                ProductVideoObjectKey: null,
                ProductImageUrls: product.ImageUrls.Take(3).ToList(),
                ScriptFullText: script.FullText,
                OutputFormat: job.OutputFormat,
                FlowType: Domain.Constants.FlowType.Avatar
            ), ct);

            variation.FinalUrl = finalKey;
            variation.Status = JobStatus.Done;

            job.Variations.Add(variation);
            job.OutputUrl = await _storage.GetPresignedUrlAsync(finalKey, ct: ct);
            job.Status = JobStatus.Done;
            job.CompletedAt = DateTime.UtcNow;
            await _jobRepo.UpdateAsync(job, ct);
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Error;
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

    private async Task Track(VideoJob job, string service, string action, decimal costUsd, CancellationToken ct)
    {
        var apiCost = new ApiCost
        {
            JobId = job.Id,
            Service = service,
            Action = action,
            CostUsd = costUsd
        };
        job.ApiCosts.Add(apiCost);
        await _jobRepo.UpdateAsync(job, ct);
    }

    private async Task<string> PollUntilDone(Func<Task<AvatarVideoResult?>> checkFunc, int timeoutMin, CancellationToken ct)
    {
        var deadline = DateTime.UtcNow.AddMinutes(timeoutMin);
        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(5000, ct);
            var result = await checkFunc();
            if (result != null)
            {
                if (result.IsSuccess)
                    return result.VideoUrl ?? throw new InvalidOperationException("HeyGen returned no video URL");
                throw new InvalidOperationException(result.ErrorMessage ?? "HeyGen rendering failed");
            }
        }
        throw new TimeoutException($"HeyGen job timed out after {timeoutMin} minutes");
    }
}
