# Kế Hoạch Implementation Chi Tiết — 7 Ngày
> Cập nhật: 2026-06-18
> Nguyên tắc: cuối ngày 7 tool chạy end-to-end, xuất được 5 MP4. Tuần 3-4 ra tiền.

---

## NGÀY 0 (Bắt Buộc Trước Khi Code)

**Output**: biết dùng voice provider nào + có test data sẵn

### Test Voice (Quan Trọng Nhất)

Đừng tin benchmark. Test thật với người thật.

```
1. Viết 1 script mẫu (30-45s):
   "Mình dùng cái này được 2 tuần rồi. Thật ra lúc đầu mình cũng không kỳ vọng
    gì nhiều lắm, nhưng sau khi thử thì... Okay, mình phải nói thật là nó ổn
    hơn mình nghĩ nhiều. Đặc biệt là phần [USP sản phẩm]. Giá thì cũng hợp lý,
    khoảng [giá]. Bạn nào đang tìm cái này thì link mua ở bio nha."

2. Chạy script qua 3 providers:
   - FPT AI:     api.fpt.ai/hmi/tts/v5 (voice: myan, leminh)
   - ElevenLabs: elevenlabs.io (voice: Vietnamese voices trong library)
   - Zalo AI:    zalo.ai/tools/ai-voice-generator

3. Cho 5-10 người nghe (không phải lập trình viên):
   → Hỏi: "Giọng nào nghe tự nhiên nhất?"

4. Implement đúng provider đó vào IVoiceSynthesisService
   → IVoiceSynthesisService đã abstract sẵn → đổi implementation không phá code
```

### Chuẩn Bị Test Data

```
Quay 2-3 video 30-60s bằng điện thoại:
- 1 video: đồ gia dụng/gadget (dễ quay nhất)
- 1 video: mỹ phẩm/skincare
- 1 video: thực phẩm/đồ ăn

→ Đây là input để test pipeline sau này. Không cần đẹp, càng thật càng tốt.
```

---

## NGÀY 1 — Project Setup + Database

**Output**: `dotnet run` không lỗi, 4 bảng trong PostgreSQL

### Bước 1: Cài Packages

```bash
cd src/Application
dotnet add package MediatR --version 12.4.1
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.11.0

cd ../Infrastructure
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.4
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.4
dotnet add package OpenAI --version 2.2.0
dotnet add package AWSSDK.S3 --version 3.7.400

cd ../Api
dotnet add package Hangfire.AspNetCore --version 1.8.14
dotnet add package Hangfire.PostgreSql --version 1.20.9
dotnet add package Swashbuckle.AspNetCore --version 7.2.0
```

### Bước 2: docker-compose.yml

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: ugcads
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
volumes:
  postgres_data:
```

```bash
docker-compose up -d
```

### Bước 3: Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHangfire(cfg =>
    cfg.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer(opt => opt.WorkerCount = 1); // WorkerCount=1 tránh FFmpeg spike

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHangfireDashboard("/hangfire");
app.MapControllers();
app.MapRazorPages();
app.Run();
```

### Bước 4: Domain Entities + Constants

Tạo theo thứ tự (xem nội dung chi tiết tại [01-database-schema.md](01-database-schema.md)):

```
src/Domain/Constants/InputType.cs    ← "video" | "images"
src/Domain/Constants/JobStatus.cs    ← pending | scripting | voicing | composing | generating | done | error
src/Domain/Constants/AiService.cs    ← openai | elevenlabs | fptai | r2

src/Domain/Entities/VideoJob.cs
src/Domain/Entities/JobBrief.cs
src/Domain/Entities/JobVariation.cs
src/Domain/Entities/ApiCost.cs
```

### Bước 5: EF Configurations + Migration

```csharp
// Ví dụ VideoJobConfiguration:
public class VideoJobConfiguration : IEntityTypeConfiguration<VideoJob>
{
    public void Configure(EntityTypeBuilder<VideoJob> builder)
    {
        builder.ToTable("video_jobs");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasOne(x => x.Brief)
               .WithOne(x => x.Job)
               .HasForeignKey<JobBrief>(x => x.JobId);
        builder.HasMany(x => x.Variations)
               .WithOne(x => x.Job)
               .HasForeignKey(x => x.JobId);
        builder.HasMany(x => x.ApiCosts)
               .WithOne(x => x.Job)
               .HasForeignKey(x => x.JobId);
    }
}
```

```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Api
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

**Kiểm tra Ngày 1:**
- [ ] `dotnet build` → không lỗi
- [ ] `docker-compose ps` → postgres Up
- [ ] `http://localhost:5000/swagger` → Swagger UI hiển thị
- [ ] 4 bảng trong DB (kiểm tra DBeaver/pgAdmin)

---

## NGÀY 2 — Application Layer + Cloudflare R2

**Output**: Interfaces, Commands, R2 upload test được

### Tạo Interfaces

Xem code đầy đủ tại [02-backend-architecture.md § Interfaces](02-backend-architecture.md):

```
src/Application/Common/Interfaces/
├── IVideoJobRepository.cs
├── IJobBriefRepository.cs
├── IContentIntelligenceService.cs   ← GenerateBriefAsync → ContentBrief
├── IVoiceSynthesisService.cs
├── IVideoComposerService.cs         ← ComposeBaseAsync + ComposeVariationAsync
└── IStorageService.cs
```

### Tạo Commands + Queries

```
src/Application/Jobs/Commands/
  CreateJob/CreateJobCommand.cs + Handler
  ProcessJobFlowD/ProcessFlowDCommand.cs + Handler (skeleton — hoàn thiện Ngày 5)

src/Application/Jobs/Queries/
  GetJob/GetJobQuery.cs + Handler
  GetJobList/GetJobListQuery.cs + Handler
```

### Implement CloudflareR2StorageService

Setup Cloudflare R2:
1. Tạo bucket `ugcads`
2. Tạo R2 API token (Read + Write)
3. Lấy endpoint URL, AccessKeyId, SecretAccessKey
4. Thêm vào appsettings.json

Xem implementation tại: [04-ai-apis-and-infrastructure.md § R2](04-ai-apis-and-infrastructure.md)

**Test R2**: upload file text nhỏ → GetPresignedUrl → mở URL trên browser.

**Kiểm tra Ngày 2:**
- [ ] `dotnet build` → không lỗi  
- [ ] R2 upload + presigned URL test OK

---

## NGÀY 3 — Content Intelligence + Voice

**Output**: GPT sinh được ContentBrief, Voice TTS ra được mp3

### Implement OpenAiContentIntelligenceService

Xem implementation + prompt đầy đủ tại: [04-ai-apis-and-infrastructure.md § ContentIntelligence](04-ai-apis-and-infrastructure.md)

**Test**: ProductDescription "máy hút bụi mini không dây 299k" → nhận JSON có:
- `hooks[]`: 5 hooks khác nhau, không giống nhau, phù hợp với sản phẩm
- `ctas[]`: 3 CTAs rõ ràng
- `voice_script`: 30-45s, đọc tự nhiên

### Implement Voice Synthesis

Dựa trên kết quả test Ngày 0, implement đúng provider:
- FPT AI: [04-ai-apis-and-infrastructure.md § FPT AI](04-ai-apis-and-infrastructure.md)
- ElevenLabs: [04-ai-apis-and-infrastructure.md § ElevenLabs](04-ai-apis-and-infrastructure.md)

**Test**: gọi SynthesizeAsync("Mình đã thử cái này rồi...") → nhận mp3 → upload R2 → play được.

**Kiểm tra Ngày 3:**
- [ ] ContentBrief JSON hợp lý với 3 sản phẩm khác nhau
- [ ] Voice mp3 nghe được, không lỗi encoding
- [ ] mp3 upload R2, presigned URL play được

---

## NGÀY 4 — FFmpeg Video Composer

**Output**: base_video.mp4 tạo được từ raw footage

### Cài FFmpeg

```bash
# Windows
winget install ffmpeg

# Kiểm tra
ffmpeg -version
```

### Implement ComposeBaseAsync

Xem FFmpeg commands tại: [04-ai-apis-and-infrastructure.md § FFmpeg](04-ai-apis-and-infrastructure.md)

**Test từng bước riêng:**

```bash
# Test 1: Scale và crop về 9:16
ffmpeg -y -i test_video.mp4 -vf "scale=1080:1920:force_original_aspect_ratio=increase,crop=1080:1920" out_scaled.mp4

# Test 2: Color grade
ffmpeg -y -i test_video.mp4 -vf "eq=brightness=0.05:saturation=1.3:contrast=1.1" out_graded.mp4

# Test 3: Mix audio (video + voice)
ffmpeg -y -i test_video.mp4 -i voice.mp3 -filter_complex "[1:a]volume=0.8[v];[0:a][v]amix=inputs=2:duration=first[aout]" -map 0:v -map "[aout]" out_mixed.mp4

# Test 4: Full compose (sau khi từng bước OK)
# Xem command đầy đủ tại 04-ai-apis-and-infrastructure.md
```

**Kiểm tra Ngày 4:**
- [ ] base_video.mp4 tạo được từ video thô
- [ ] Color grade trông rõ ràng khác biệt so với original
- [ ] Caption sync với voiceover (nếu có SRT)
- [ ] 9:16 đúng kích thước, play được trên điện thoại

---

## NGÀY 5 — Pipeline End-to-End (ProcessFlowDCommandHandler)

**Output**: Upload 1 video → nhận 5 MP4 download được

### Implement ComposeVariationAsync

```bash
# Test variation overlay:
ffmpeg -y -i base_video.mp4 \
  -filter_complex "[0:v]drawtext=text='Test hook text đây':fontsize=36:fontcolor=white:x=(w-tw)/2:y=h*0.15:box=1:boxcolor=black@0.65:boxborderw=12:enable='between(t,0,3)'[vout]" \
  -map "[vout]" -map 0:a -c:v libx264 -preset fast -crf 20 -c:a copy variation_test.mp4
```

### Hoàn Thiện ProcessFlowDCommandHandler

Xem code đầy đủ tại: [02-backend-architecture.md § ProcessFlowDCommandHandler](02-backend-architecture.md)

**Test end-to-end (quan trọng nhất):**
```
1. Upload video điện thoại 30s qua API
2. Tạo job → Hangfire enqueue
3. Chờ pipeline chạy (theo dõi logs)
4. Check job status: pending → scripting → voicing → composing → generating → done
5. Download 5 MP4 → xem từng video
6. Kiểm tra: mỗi video có hook text overlay khác nhau ở 3s đầu
7. Check api_costs table: tổng ~$0.047/job
```

**Kiểm tra Ngày 5:**
- [ ] 1 video → 5 MP4 download được
- [ ] Mỗi variation có hook text khác nhau
- [ ] Status transition đúng thứ tự
- [ ] API cost ~$0.047 (tổng openai + elevenlabs/fptai)
- [ ] 3 jobs liên tiếp không lỗi

---

## NGÀY 6 — Razor Pages UI

**Output**: Internal tool có UI đẹp đủ để demo cho khách

### Tạo Pages

Xem HTML + PageModel đầy đủ tại: [03-frontend-architecture.md](03-frontend-architecture.md)

```
src/Api/Pages/
├── _Layout.cshtml
├── Jobs/
│   ├── Index.cshtml + Index.cshtml.cs
│   ├── New.cshtml + New.cshtml.cs
│   └── Detail/{id}.cshtml + {id}.cshtml.cs
└── Dashboard/
    └── Index.cshtml
```

**Ưu tiên:**
1. Jobs/New — form upload, phải dễ dùng
2. Jobs/Detail — progress bar + 5 video grid + download
3. Jobs/Index — danh sách, auto-refresh
4. Dashboard — cost tracking (có thể làm sau)

**Test UI với người thật (không phải IT):**
- Cho họ tự dùng không giải thích
- Quan sát chỗ nào họ bị stuck
- Fix trước khi demo cho khách

**Kiểm tra Ngày 6:**
- [ ] Upload form hoạt động (video + ảnh)
- [ ] Progress bar hiển thị đúng bước
- [ ] 5 video có thể xem + download
- [ ] Auto-refresh khi job đang chạy

---

## NGÀY 7 — Deploy Railway + Test End-to-End Thật

**Output**: Tool chạy trên internet, sẵn sàng nhận đơn đầu tiên

### Deploy

Tạo files:

```toml
# nixpacks.toml
[phases.setup]
nixPkgs = ["ffmpeg"]
```

```toml
# railway.toml
[build]
builder = "NIXPACKS"
buildCommand = "dotnet publish src/Api -c Release -o /publish"

[deploy]
startCommand = "dotnet /publish/Api.dll"
restartPolicyType = "ON_FAILURE"
```

```bash
railway login
railway init
railway add --database postgresql
railway up
```

Set environment variables trên Railway dashboard:
```
OpenAI__ApiKey=sk-...
ElevenLabs__ApiKey=...    (hoặc FptAi__ApiKey=...)
Cloudflare__R2__Endpoint=...
Cloudflare__R2__AccessKeyId=...
Cloudflare__R2__SecretAccessKey=...
Cloudflare__R2__BucketName=ugcads
FFmpeg__Path=ffmpeg
```

### Test End-to-End Thật (Ngày 7)

```
Test 1 — Video thô (sản phẩm thật):
  Quay 30s bằng điện thoại → upload lên Railway URL
  → 5 MP4 trong 10 phút
  → Kiểm tra: hook text rõ, voice tự nhiên, video không lỗi

Test 2 — Ảnh sản phẩm:
  Upload 3 ảnh → 5 MP4 slideshow
  → Kiểm tra: Ken Burns effect, transitions mượt

Test 3 — Stress test:
  Tạo 3 jobs cùng lúc → Hangfire queue xử lý lần lượt
  → Không crash, không timeout
```

**Kiểm tra Ngày 7:**
- [ ] Railway URL accessible
- [ ] Upload video → 5 MP4 trên production
- [ ] Logs không có error lạ
- [ ] PostgreSQL connection ổn định

---

## TUẦN 3-4: RA TIỀN

### Làm Video Mẫu (Ngày 8-9)

```
Mục tiêu: 3-5 video mẫu để show portfolio

Quy trình:
1. Quay 3-5 sản phẩm thật (điện thoại, không cần studio)
2. Chạy qua tool → lấy 5 variations tốt nhất
3. Test "người thật": cho 5 người xem từng video → "bạn có click link này không?"
4. Nếu pass → đưa vào portfolio

Không cần video hoàn hảo. Cần video đủ tốt để seller thấy giá trị.
```

### Post Tìm Khách (Ngày 10-11)

**Facebook Groups (miễn phí):**
```
Nhóm đề xuất: "Kinh doanh Online VN", "TikTok Shop Seller VN", "Dropship Việt Nam"

Template post:
"Mình đang test tool AI tạo 5 biến thể video quảng cáo từ 1 clip điện thoại.
Bạn quay 30 giây thô → mình trả về 5 video khác hook để test TikTok Ads, MIỄN PHÍ.
[Đính kèm 2 video before/after thực tế]
Ai muốn thử DM mình nhé. Chỉ nhận 5 người đầu tiên."

Goal: 10 người thử → 5 feedback thật → 2-3 người trả tiền lần sau
```

**Direct Message (hiệu quả hơn group post):**
```
Tìm seller đang chạy ads TikTok → tìm Facebook page của họ → DM:
"Mình thấy anh/chị đang chạy ads sản phẩm X.
Mình có thể biến clip điện thoại của anh/chị thành 5 video ads khác hook
trong 10 phút, free để test. Anh/chị chỉ cần quay 30 giây unboxing là xong."
```

### Thu Tiền (Ngày 15+)

```
Bảng giá đơn giản:
- 1 job (5 videos): 199k VND ← thử
- 3 job (15 videos): 500k VND
- Gói tháng 10 job: 1.8tr VND ← best value

Nhận tiền qua: Momo / Banking / Zalo Pay
Giao hàng: upload lên Google Drive hoặc share presigned URLs
```

---

## PHASE AUTOMATION (Tháng 2-3, Khi Có 10+ Khách)

### Google Form + Make.com

```
1. Tạo Google Form:
   - Product description (textarea)
   - Upload video/ảnh (file upload)
   - Voice preference
   - Email nhận file

2. Make.com (free tier, 1000 ops/tháng):
   Google Form → New Response trigger
   → HTTP POST /api/jobs (với form data)
   → Hangfire enqueue tự động

3. SendGrid (free tier, 100 emails/ngày):
   Job done → webhook → SendGrid
   → Email tới customer_email với 5 download links
```

### Stripe Billing (Khi Có Gói Tháng)

```
- Tạo Stripe account → Products: Gói Tháng 10 Job (1.8tr VND)
- Checkout link cho từng gói
- Khi payment succeed → Stripe webhook → tạo order trong DB
```

---

## PHASE SAAS (Tháng 5+, Chỉ Khi Có 20 Paying Customers)

```
Checklist bắt buộc trước khi bắt đầu:
☐ 20+ paying customers đang dùng đều đặn
☐ Bạn biết đúng workflow khách cần (vì đã làm tay nhiều lần)
☐ Revenue từ service đủ pay cho infrastructure SaaS ($25 Clerk + $X Stripe + hosting)

Khi đủ điều kiện, thêm vào codebase hiện tại:
1. Clerk.Net SDK (auth — không tự build)
2. Stripe subscription
3. Users table + quota check middleware
4. Self-serve Razor Pages → Next.js rewrite sau
```

---

## Timeline Tổng Kết

| Ngày/Tuần | Output | Checklist |
|-----------|--------|-----------|
| **Ngày 0** | Voice provider tested, test data sẵn | ☐ |
| **Ngày 1** | DB 4 bảng, `dotnet run` OK | ☐ |
| **Ngày 2** | Interfaces, Commands, R2 upload OK | ☐ |
| **Ngày 3** | GPT sinh ContentBrief OK, Voice TTS OK | ☐ |
| **Ngày 4** | FFmpeg base_video.mp4 OK | ☐ |
| **Ngày 5** | **1 video → 5 MP4 end-to-end** | ☐ |
| **Ngày 6** | Razor Pages UI hoàn chỉnh | ☐ |
| **Ngày 7** | Railway deployed, test thật OK | ☐ |
| Tuần 3 | 3 video mẫu, 10 free users | ☐ |
| Tuần 4 | **2-3 paying customers đầu tiên** | ☐ |
| Tháng 2 | 10 khách, automation email | ☐ |
| Tháng 3 | 20 khách, gói tháng ổn định | ☐ |
| Tháng 5+ | **Bắt đầu SaaS layer** | ☐ |

---

## Nếu Bị Blocked

| Block | Giải pháp |
|-------|-----------|
| Voice nghe như robot | Đổi provider ngay (test Ngày 0 để tránh điều này) |
| Seller không có raw footage | Mode ảnh (slideshow + voiceover) — không cần video thô |
| Caption sync lệch | ElevenLabs timestamps API hoặc tự tính từ word count (1 word = 0.3s) |
| FFmpeg crash | Test từng command riêng, log stderr vào file, test với video ngắn trước |
| Railway OOM | WorkerCount=1 (đã set), giảm video resolution xuống 720×1280 nếu cần |
| GPT trả JSON sai | Thêm JSON schema validation, retry 1 lần khi parse fail |
| Không có khách | DM trực tiếp 20 seller đang chạy ads — targeted hơn nhiều so với group post |
