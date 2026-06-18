# Master Implementation Plan — Creative Variation Engine
> Tài liệu tổng hợp từ tất cả file trong Claude-Plans/.
> Cập nhật: 2026-06-18 — Đã align với chiến lược mới trong 10-product-strategy.md.
> Mỗi task có link đến đúng file/section để tra cứu chi tiết.

---

## MỤC TIÊU SẢN PHẨM

> **1 câu**: Gửi 1 video hoặc 3 ảnh sản phẩm → nhận **5 video quảng cáo khác nhau hook** để test TikTok Ads trong 10 phút.

```
Seller bán gì:  "Mình thuê người tạo 5 biến thể video để test ads"
Bạn làm gì:    GPT sinh hooks → ElevenLabs TTS → FFmpeg compose base → FFmpeg tạo 5 MP4
Cost/job:       ~$0.047 (~1.2k VND)
Charge/job:     199k-250k VND
Margin:         99.4%
```

---

## TÌNH TRẠNG CODEBASE (Trước Khi Bắt Đầu)

```
ĐÃ CÓ (giữ lại):
✓ src/Infrastructure/AI/OpenAiScriptGeneratorService.cs   → mở rộng thành ContentIntelligence
✓ src/Infrastructure/AI/ElevenLabsVoiceSynthesisService.cs → giữ nguyên
✓ src/Infrastructure/Video/FfmpegVideoComposerService.cs   → thêm ComposeBaseAsync + ComposeVariationAsync
✓ src/Infrastructure/Storage/CloudflareR2StorageService.cs → giữ nguyên
✓ src/Infrastructure/DependencyInjection.cs               → cập nhật registrations
✓ Hangfire setup                                          → giữ nguyên, WorkerCount=1

DEPRIORITIZE (giữ code, không build thêm):
~ src/Infrastructure/AI/HeyGenAvatarVideoService.cs       ← Flow A, dùng sau V6
~ src/Infrastructure/AI/KlingService.cs                   ← Flow B/C, dùng sau V6
~ src/Infrastructure/AI/ShopeeProductScraper.cs           ← để V4+
~ ProcessJobFlowA/B/CCommandHandler.cs                    ← không build thêm

CẦN TẠO MỚI:
+ Domain Entities: VideoJob, JobBrief, JobVariation, ApiCost
+ Domain Constants: InputType, JobStatus (mới), AiService
+ IContentIntelligenceService + OpenAiContentIntelligenceService
+ ProcessJobFlowD/ (Command + Handler) — pipeline 4 bước chính
+ CreateJobCommand / GetJobQuery / GetJobListQuery
+ Upload API endpoint
+ Razor Pages: Jobs/Index, Jobs/New, Jobs/Detail
```

---

## PHASE 0 — Dọn Dẹp & Project Setup
**Mục tiêu**: `dotnet run` không lỗi, PostgreSQL kết nối được
**Thời gian**: Ngày 1 (bắt đầu sau khi test voice xong)
**Tham khảo**: [05-phase-implementation.md § PHASE 0](05-phase-implementation.md)

### Checklist

- [ ] **Ngày 0 (bắt buộc trước khi code)**
  - Test voice tiếng Việt: chạy 1 script qua FPT AI + Zalo AI + ElevenLabs → cho 5-10 người nghe → chọn provider
  - `IVoiceSynthesisService` đã abstract sẵn → swap vendor không phá code
  - Quay 2-3 video thô bằng điện thoại với sản phẩm thật → test data sẵn

- [ ] **Xoá/clean code cũ không cần**
  - Xoá WeatherForecastController nếu còn
  - Giữ nguyên HeyGen/Kling/Shopee code — chỉ đừng build thêm

- [ ] **Cài NuGet packages** (xem [05-phase-implementation.md § Bước 2](05-phase-implementation.md))
  ```
  Application:    MediatR 12.4.1 · FluentValidation 11.11.0
  Infrastructure: Npgsql.EFCore.PostgreSQL 9.0.4 · EFCore.Design 9.0.4
                  OpenAI 2.2.0 · AWSSDK.S3 3.7.400
  Api:            Hangfire.AspNetCore 1.8.14 · Hangfire.PostgreSql 1.20.9
                  Swashbuckle.AspNetCore 7.2.0
  ```

- [ ] **Tạo docker-compose.yml** (PostgreSQL 16)
  Xem nội dung tại: [05-phase-implementation.md § PHASE 0](05-phase-implementation.md)

- [ ] **Cập nhật Program.cs** với skeleton mới
  Cần có: AddControllers, AddRazorPages, AddSwaggerGen, AddApplication, AddInfrastructure,
  AddHangfire (UsePostgreSqlStorage), AddHangfireServer (WorkerCount=1)

- [ ] **Cập nhật appsettings.json**
  Keys cần: OpenAI, ElevenLabs (hoặc FPT AI), Cloudflare R2
  Xem schema tại: [04-ai-apis-and-infrastructure.md § appsettings](04-ai-apis-and-infrastructure.md)

### Kiểm tra Phase 0
- [ ] `dotnet build` — không lỗi
- [ ] `docker-compose ps` — postgres Up
- [ ] `http://localhost:5000/swagger` — Swagger UI hiển thị

---

## PHASE 1 — Domain Layer & Database
**Mục tiêu**: 4 bảng trong DB, migration chạy được
**Thời gian**: Ngày 1-2
**Tham khảo chính**: [01-database-schema.md](01-database-schema.md), [05-phase-implementation.md § PHASE 1](05-phase-implementation.md)

### 1.1 — Domain Constants

Tạo `src/Domain/Constants/`:

| File | Nội dung |
|------|----------|
| `InputType.cs` | Video \| Images |
| `JobStatus.cs` | Pending → Scripting → Voicing → Composing → Generating → Done \| Error |
| `AiService.cs` | openai \| elevenlabs \| r2 |

### 1.2 — Domain Entities

Tạo `src/Domain/Entities/` theo thứ tự FK:

| File | FK dependencies | Xem tại |
|------|----------------|---------|
| `VideoJob.cs` | không có | [01-database-schema.md § Bảng 1](01-database-schema.md) |
| `JobBrief.cs` | VideoJob | [01-database-schema.md § Bảng 2](01-database-schema.md) |
| `JobVariation.cs` | VideoJob | [01-database-schema.md § Bảng 3](01-database-schema.md) |
| `ApiCost.cs` | VideoJob | [01-database-schema.md § Bảng 4](01-database-schema.md) |

**Lưu ý**: `HooksJson` và `CtasJson` trong `JobBrief` lưu dạng `JsonDocument` (JSONB trong PostgreSQL).

### 1.3 — EF Core Configurations

Tạo `src/Infrastructure/Persistence/Configurations/`:

| File | Điểm đặc biệt |
|------|--------------|
| `VideoJobConfiguration.cs` | index trên Status, CreatedAt |
| `JobBriefConfiguration.cs` | HooksJson/CtasJson → `.HasColumnType("jsonb")`; HasOne-WithOne với VideoJob |
| `JobVariationConfiguration.cs` | FK VideoJob cascade; index trên JobId, VariationIndex |
| `ApiCostConfiguration.cs` | FK VideoJob cascade; CostUsd → decimal(10,6) |

Xem chi tiết tại: [05-phase-implementation.md § PHASE 1](05-phase-implementation.md)

**Cập nhật AppDbContext**:
```csharp
DbSet<VideoJob>, DbSet<JobBrief>, DbSet<JobVariation>, DbSet<ApiCost>
```
Xem code tại: [01-database-schema.md § AppDbContext](01-database-schema.md)

### 1.4 — Migration

```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Api
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

### Kiểm tra Phase 1
- [ ] 4 bảng tạo đúng (kiểm tra DBeaver/pgAdmin)
- [ ] `dotnet build` — không lỗi

---

## PHASE 2 — Application Layer (Interfaces + Commands)
**Mục tiêu**: Architecture đủ để implement Infrastructure
**Thời gian**: Ngày 2 (song song với Phase 1)
**Tham khảo chính**: [02-backend-architecture.md](02-backend-architecture.md)

### 2.1 — Interfaces

Tạo `src/Application/Common/Interfaces/`:

| Interface | Mục đích | Xem code tại |
|-----------|---------|--------------|
| `IVideoJobRepository.cs` | GetById, Add, Update | [02-backend-architecture.md](02-backend-architecture.md) |
| `IJobBriefRepository.cs` | Add, Update | [02-backend-architecture.md](02-backend-architecture.md) |
| `IContentIntelligenceService.cs` | GenerateBriefAsync → ContentBrief | [02-backend-architecture.md § IContentIntelligenceService](02-backend-architecture.md) |
| `IVoiceSynthesisService.cs` | SynthesizeAsync + GetAvailableVoicesAsync | [02-backend-architecture.md](02-backend-architecture.md) |
| `IVideoComposerService.cs` | ComposeBaseAsync + ComposeVariationAsync | [02-backend-architecture.md § IVideoComposerService](02-backend-architecture.md) |
| `IStorageService.cs` | Upload + GetPresignedUrl + Download + Delete | [02-backend-architecture.md](02-backend-architecture.md) |

**Record types cần tạo kèm**: `ContentBrief(string[] Hooks, string[] Ctas, string VoiceScript)`

### 2.2 — Commands & Queries

Tạo `src/Application/Jobs/`:

```
Commands/
  CreateJob/
    CreateJobCommand.cs          ← ProductDescription, InputType, InputObjectKey, VoiceId, CustomerEmail, Notes
    CreateJobCommandHandler.cs   ← tạo VideoJob → enqueue ProcessFlowD → return Guid

  ProcessJobFlowD/               ← CORE PIPELINE — 4 bước
    ProcessFlowDCommand.cs       ← JobId, VoiceId
    ProcessFlowDCommandHandler.cs ← ContentIntelligence → Voice → BaseCompose → 5 Variations

Queries/
  GetJob/
    GetJobQuery.cs + Handler     ← trả VideoJobDto (include Brief, Variations, ApiCosts)
  GetJobList/
    GetJobListQuery.cs + Handler ← filter Status; phân trang
```

Xem code handler chi tiết tại:
- **ProcessFlowDCommandHandler**: [02-backend-architecture.md § ProcessFlowDCommandHandler](02-backend-architecture.md)
- **CreateJobCommandHandler**: [02-backend-architecture.md § CreateJobCommandHandler](02-backend-architecture.md)

### 2.3 — Upload API + JobsController

Tạo `src/Api/Controllers/`:
- `UploadController.cs` — `POST /api/upload` → nhận multipart → upload R2 → trả object_key
- `JobsController.cs` — `POST /api/jobs`, `GET /api/jobs/{id}`, `GET /api/jobs`

Xem code tại: [02-backend-architecture.md § Controllers](02-backend-architecture.md)

---

## PHASE 3 — Infrastructure: AI Services & Storage
**Mục tiêu**: Từng service test được độc lập
**Thời gian**: Ngày 2-4
**Tham khảo chính**: [04-ai-apis-and-infrastructure.md](04-ai-apis-and-infrastructure.md)

### 3.1 — Cloudflare R2 Storage (LÀM TRƯỚC TIÊN)

Tạo `src/Infrastructure/Storage/CloudflareR2StorageService.cs`

**Test**: Upload file nhỏ → GetPresignedUrl → mở URL trên browser.
Xem implementation tại: [04-ai-apis-and-infrastructure.md § R2](04-ai-apis-and-infrastructure.md)

### 3.2 — OpenAI Content Intelligence (IContentIntelligenceService)

Tạo `src/Infrastructure/AI/OpenAiContentIntelligenceService.cs`

**Quan trọng**: GPT tự chọn angle hay nhất cho từng sản phẩm — không hard-code 5 loại hook.
Xem prompt tại: [04-ai-apis-and-infrastructure.md § ContentIntelligence](04-ai-apis-and-infrastructure.md)

**Test**: ProductDescription "máy hút bụi mini" → nhận JSON có hooks[5], ctas[3], voiceScript.

### 3.3 — Voice Synthesis

**Tạo** (hoặc cập nhật) service dựa trên kết quả test Ngày 0:
- FPT AI: `src/Infrastructure/AI/FptAiVoiceSynthesisService.cs`
- ElevenLabs: `src/Infrastructure/AI/ElevenLabsVoiceSynthesisService.cs` (đã có)

Xem implementation tại: [04-ai-apis-and-infrastructure.md § Voice](04-ai-apis-and-infrastructure.md)

**Test**: Script 30s → nhận mp3 → upload R2 → play được, giọng tự nhiên.

### 3.4 — FFmpeg Video Composer

Cập nhật `src/Infrastructure/Video/FfmpegVideoComposerService.cs` với 2 method mới:

| Method | Input | Output |
|--------|-------|--------|
| `ComposeBaseAsync` | raw video/ảnh + voiceover | base_video.mp4 trên R2 |
| `ComposeVariationAsync` | base_video + hook text | variation_N.mp4 trên R2 |

Xem FFmpeg commands chi tiết tại: [04-ai-apis-and-infrastructure.md § FFmpeg](04-ai-apis-and-infrastructure.md)

**Test ComposeBase**: video điện thoại + mp3 → base_video.mp4, color grade ok, caption sync.
**Test ComposeVariation**: base_video × "Hook text" → variation_0.mp4, text overlay đầu video đúng.

### 3.5 — DependencyInjection.cs

Cập nhật `src/Infrastructure/DependencyInjection.cs`.
Xem code đầy đủ tại: [02-backend-architecture.md § DependencyInjection](02-backend-architecture.md)

---

## PHASE 4 — End-to-End Pipeline (Flow D)
**Mục tiêu**: Upload 1 video → nhận 5 MP4 download được
**Thời gian**: Ngày 5
**Tham khảo chính**: [02-backend-architecture.md § ProcessFlowDCommandHandler](02-backend-architecture.md), [06-video-generation-approaches.md](06-video-generation-approaches.md)

### Pipeline 4 bước:

```
STEP 1 — Content Intelligence (GPT-4o)
  Input: product_description + input_type
  Output: hooks[5], ctas[3], voiceScript → lưu vào job_briefs

STEP 2 — Voice Synthesis
  Input: voiceScript → ElevenLabs/FPT AI
  Output: voiceover.mp3 → upload R2 → update job_briefs.voice_url

STEP 3 — Base Composition (FFmpeg)
  Input: raw video hoặc slideshow ảnh + voiceover.mp3
  Output: base_video.mp4 (color grade + captions + music) → R2

STEP 4 — Variation Loop (FFmpeg × 5, chạy song sang Task.WhenAll)
  base_video × hooks[0] → variation_0.mp4
  base_video × hooks[1] → variation_1.mp4
  base_video × hooks[2] → variation_2.mp4
  base_video × hooks[3] → variation_3.mp4
  base_video × hooks[4] → variation_4.mp4
  → Upload 5 files → update job_variations
```

### Kiểm tra Phase 4
- [ ] Upload video 30s → 5 MP4 trong < 10 phút
- [ ] Mỗi variation có hook text overlay khác nhau đầu video
- [ ] Color grade trông tốt hơn original
- [ ] Caption sync với voiceover
- [ ] API cost ~$0.047/job (check api_costs table)
- [ ] 3 jobs liên tiếp không lỗi

---

## PHASE 5 — Razor Pages UI
**Mục tiêu**: Internal tool có UI dùng được để nhận và giao đơn
**Thời gian**: Ngày 6
**Tham khảo chính**: [03-frontend-architecture.md](03-frontend-architecture.md)

### Pages cần tạo

```
src/Api/Pages/
├── _Layout.cshtml         ← MVP.css (classless, 1 dòng link)
├── Index.cshtml           ← redirect → /Jobs
├── Jobs/
│   ├── Index.cshtml       ← danh sách + filter status
│   ├── New.cshtml         ← form: upload video/ảnh + mô tả sản phẩm
│   └── Detail/{id}        ← progress 4 bước + 5 video player/download
└── Dashboard/
    └── Index.cshtml       ← cost tracking tổng ngày/tháng
```

### Chi tiết từng page
- **Jobs/Index**: bảng jobs, badge status, auto-refresh 5s khi có job đang chạy
- **Jobs/New**: upload form (video hoặc ảnh), input product description, chọn voice
- **Jobs/Detail**: progress bar 4 bước → khi done: 5 video player + 5 download button
- **Dashboard**: tổng jobs, tổng cost, gross margin

Xem HTML + PageModel đầy đủ tại: [03-frontend-architecture.md](03-frontend-architecture.md)

---

## PHASE 6 — Deploy & Ra Tiền
**Mục tiêu**: Tool chạy trên Railway, nhận đơn qua form, giao video qua email
**Thời gian**: Ngày 7 + Tuần 3-4
**Tham khảo chính**: [10-product-strategy.md § PHẦN 3](10-product-strategy.md), [05-phase-implementation.md § PHASE 5](05-phase-implementation.md)

### Ngày 7: Deploy Railway
Xem `railway.toml` + `nixpacks.toml` tại: [04-ai-apis-and-infrastructure.md § Railway](04-ai-apis-and-infrastructure.md)

```bash
railway login && railway init
railway add --database postgresql
railway up
```

### Tuần 3-4: Ra Tiền
```
Ngày 8-9:   Làm 3-5 video mẫu với sản phẩm thật → show portfolio
Ngày 10-11: Post 3 Facebook Groups (Kinh doanh Online VN, TikTok Shop Seller VN...)
            → offer 1 job FREE cho đầu tiên inbox
Ngày 12-14: DM 10 seller đang chạy ads TikTok → offer free để test
Ngày 15+:   → Nhận order đầu tiên trả tiền (199k/job)
```

### Bảng giá (Giai đoạn 1)
```
1 job (5 videos):        199k-250k VND  ← thử
3 job (15 videos):       500k VND       ← 15% discount
10 job (50 videos):      1.5tr VND      ← 25% discount
Gói tháng (10 job):      1.8tr VND      ← best value
Gói agency:              6tr VND/tháng
```

### Kiểm tra Phase 6
- [ ] 3 video mẫu pass test người thật ("bạn có click link này không?")
- [ ] Railway deploy thành công, URL accessible
- [ ] 3 paying orders đầu tiên

---

## PHASE 7 — Scale (Tháng 2-3+)
**CHỈ bắt đầu sau khi có 10+ khách trả tiền đều đặn**
**Tham khảo**: [10-product-strategy.md § PHẦN 4-5](10-product-strategy.md)

### Tháng 2-3: Productized Service
- Gói tháng + Stripe invoicing
- Google Form intake → Make.com webhook → auto-queue job
- SendGrid: job done → auto email 5 download links
- Batch order: upload CSV nhiều sản phẩm → queue tất cả

### Tháng 4+: SEA Expansion
- Thêm ngôn ngữ voice (Indonesia/Malaysia — ElevenLabs hỗ trợ)
- Price tương đương hoặc cao hơn VN

### Tháng 5+: SaaS Layer (chỉ khi có 20 paying customers)
```
Thêm vào:
├── Auth (Clerk - $25/tháng)
├── Billing (Stripe subscription)
├── Self-serve UI (user tự upload)
├── Dashboard per-user
└── Usage limits (free/starter/pro/agency)

Giữ nguyên:
├── Content Intelligence pipeline
├── Voice synthesis
├── FFmpeg variation engine
└── Cloudflare R2 storage
```

SaaS pricing: Free (1 job/tháng) · Starter $19 (10 job) · Pro $49 (30 job) · Agency $149 (100 job)

---

## Chi Phí API Per Job (5 Variations)

| Service | Action | Cost |
|---------|--------|------|
| GPT-4o | hooks + CTAs + script | $0.015 |
| ElevenLabs | TTS 30-45s | $0.030 |
| FFmpeg | tự host | $0 |
| Cloudflare R2 | 5 MP4 files | $0.002 |
| **TOTAL** | **1 job = 5 videos** | **~$0.047** |

**Charge 200k VND/job → margin 99.4%**

---

## Nếu Bị Blocked

| Block | Giải pháp |
|-------|-----------|
| Voice nghe như robot | Đổi provider (FPT AI → Zalo AI → ElevenLabs) — test Ngày 0 |
| Seller không có raw footage | Mode ảnh (slideshow) — không cần video thô |
| Caption sync lệch | ElevenLabs timestamps API hoặc tính từ word count |
| FFmpeg lỗi | Test từng command riêng, log stderr |
| Railway CPU spike | WorkerCount=1 đã set sẵn, đủ cho MVP |
| Không có khách | DM trực tiếp seller đang chạy ads TikTok |
| Khách không trả tiền | Job đầu free, charge từ job thứ 2 |

---

## Timeline Tổng Kết

| Phase | Thời gian | Output | Revenue |
|-------|----------|--------|---------|
| 0: Setup | Ngày 0-1 | `dotnet run` + voice tested | $0 |
| 1: Database | Ngày 1-2 | 4 bảng DB | $0 |
| 2: App Layer | Ngày 2 | Interfaces + Commands | $0 |
| 3: Infrastructure | Ngày 2-4 | AI services test được riêng | $0 |
| 4: **Pipeline** (Core) | Ngày 5 | **1 video → 5 MP4 download** | $0 |
| 5: UI | Ngày 6 | Razor Pages internal tool | $0 |
| 6: Deploy + Ra tiền | Ngày 7 + Tuần 3-4 | Railway up + 3 paying orders | ~1-2tr VND |
| 7: Productized | Tháng 2-3 | Gói tháng, email auto | ~8-12tr VND/tháng |
| 8: SaaS | Tháng 5+ (khi có 20 khách) | Auth + Billing + self-serve | ~30tr+ VND/tháng |

---

## Roadmap Feature (Chỉ Build Khi Có Điều Kiện)

```
V1  (Ngay)         Upload → 5 variations → Ship
V2  (10 khách)     Controlled A/B: thay đúng 1 biến mỗi lần → seller đọc được kết quả
V3  (30 khách)     Hook library theo ngành từ data thật (seller báo "V3 thắng")
V4  (100 khách)    Ad Strategy Engine — có data thật mới có ý nghĩa
V5  (doanh thu ổn) Competitor Intelligence — link TikTok đối thủ → phân tích
V6  (Phase 3)      Virtual Try-On (Kling) — chỉ fashion, khi revenue đủ absorb risk
V7  (nếu cần)      Product Animation — chỉ build khi AI video đủ ổn định
```

> V4 trở đi cần data thật. Không có user = không có data = chỉ là GPT hallucination.
