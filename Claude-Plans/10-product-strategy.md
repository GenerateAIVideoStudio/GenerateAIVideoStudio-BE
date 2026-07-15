# Chiến Lược Sản Phẩm & Kiếm Tiền (Bootstrap, Vốn Ít)

> Cập nhật: 2026-06-18
> Mục tiêu: Dùng tool để kiếm tiền trước, rồi mới bán tool cho người khác.

---

## TÓM TẮT CHIẾN LƯỢC (đọc cái này trước)

```
Giai đoạn 1 (tháng 1-2):  Tự dùng tool để nhận job → kiếm tiền ngay
Giai đoạn 2 (tháng 3-4):  Bán package dịch vụ → scale thu nhập
Giai đoạn 3 (tháng 5+):   Chuyển thành SaaS → người khác tự dùng → passive income
```

**Tại sao làm theo thứ tự này?**
- Giai đoạn 1: không cần auth, billing, onboarding — tiết kiệm 80% thời gian build
- Giai đoạn 1: revenue trong tuần 3-4, không phải tháng 6-7
- Giai đoạn 1-2: bạn hiểu *thật sự* khách hàng cần gì trước khi build SaaS
- Giai đoạn 1-2: dùng revenue để trả API cost, không đốt vốn

---

## PHẦN 1 — Vấn Đề Của Kế Hoạch Cũ

| Vấn đề | Chi tiết |
|--------|---------|
| Định vị mơ hồ | "1 video đẹp" = không phải thứ seller thật sự cần |
| Phụ thuộc AI vendor | HeyGen/Kling đắt tiền, dễ bị ảnh hưởng khi vendor thay đổi giá |
| Quá nhiều phase | 9 tháng build mới bán được — trong khi thị trường AI thay đổi nhanh |
| Không có "aha moment" | User nhận 1 video → không biết video đó có hiệu quả không |
| Không phù hợp vốn ít | Kling/HeyGen = $0.14-1.00/video vs FFmpeg = $0.05/video |

**Insight cốt lõi từ phân tích lại:**
> Seller không hỏi "video AI trông đẹp không?"
> Seller hỏi: **"Tôi có đủ góc quảng cáo để test ads không?"**

---

## PHẦN 2 — Sản Phẩm Mới: Creative Variation Engine

### Một câu mô tả

> Gửi 1 video hoặc 3 ảnh sản phẩm → nhận **5 video quảng cáo khác nhau** để test TikTok Ads trong 10 phút.

### Tại sao đây là cơ hội thật

**UGC Ads + A/B Testing = nhu cầu thực tế của seller 2025:**
- Seller cần **nhiều góc quảng cáo** để test, không phải 1 video chỉn chu
- TikTok Ads thuật toán ưu tiên content trông tự nhiên như người thật
- Thuê UGC creator thật: $50-300/video, mất 5-7 ngày — không thể test nhanh
- Tool hiện tại trên thị trường: cho 1 video, không cho bộ variations có thể so sánh

**Cái gap thị trường:**
- HeyGen/Synthesia: 1 video đắt, trông "corporate", không fit TikTok Shop VN
- Runway/Kling: generate video chung chung, không có workflow cho ads testing
- **Gap**: end-to-end pipeline tạo 5 variations có thể so sánh, nhanh, rẻ — chưa ai làm tốt

### Controlled Variation — Thay 1 biến mỗi lần

```
V1: Hook A + CTA gốc + Voice gốc          ← baseline
V2: Hook B + CTA gốc + Voice gốc          ← chỉ đổi hook
V3: Hook C + CTA gốc + Voice gốc          ← chỉ đổi hook
V4: Hook A + CTA mới + Voice gốc          ← chỉ đổi CTA
V5: Hook A + CTA gốc + Voice nhanh hơn   ← chỉ đổi pace
```

Seller test được từng biến → biết chính xác cái gì hiệu quả. Đây là thứ họ sẵn sàng trả tiền.

---

## PHẦN 3 — Giai Đoạn 1: Kiếm Tiền Bằng Cách Tự Làm Dịch Vụ

### Concept: "AI-Powered Ad Variation Service"

Bạn không bán tool — bạn **bán khả năng tìm ra góc quảng cáo thắng**.

```
Khách hàng thấy:  "Mình thuê người tạo 5 biến thể video để test ads"
Thực tế:          Bạn dùng tool → xuất 5 MP4 → giao hàng trong 10 phút
```

### Flow làm việc (Flow D — Core Flow)

```
INPUT
  Seller upload: 1 video (30-60s) hoặc 3-5 ảnh sản phẩm
       ↓
STEP 1 — Content Intelligence (GPT-4o)
  Phân tích sản phẩm → sinh:
    hooks[]   : 5 hooks phù hợp nhất với loại sản phẩm (GPT tự chọn angle,
                không hard-code. VD: máy hút bụi → pain mạnh; nước hoa → desire)
    ctas[]    : 3 CTA options
    script    : voiceover script 30-45s
       ↓
STEP 2 — Voice Synthesis (ElevenLabs)
  script → voiceover.mp3
       ↓
STEP 3 — Base Composition (FFmpeg)
  video/ảnh + voice + caption + nhạc nền → base_video.mp4
       ↓
STEP 4 — Variation Generator (FFmpeg)
  base_video × hooks[0] → V1
  base_video × hooks[1] → V2
  base_video × hooks[2] → V3
  base_video × hooks[3] → V4
  base_video × hooks[4] → V5
       ↓
OUTPUT
  5 file MP4 sẵn sàng upload TikTok Ads / Meta Ads
```

> **Pitch với khách hàng**: "Anh/chị chỉ cần quay 30 giây unboxing sản phẩm.
> Không cần đẹp, không cần edit gì. Em trả về 5 video khác hook trong 10 phút.
> Anh/chị biết hook nào convert tốt nhất sau 2 ngày test."

### Flow dự phòng (khi không có video thô)

```
Khách gửi: 3-5 ảnh sản phẩm
    ↓
FFmpeg slideshow: ảnh + ken burns effect + voiceover + caption
    ↓
5 variations với hook khác nhau
```

**Không dùng Kling/HeyGen ở giai đoạn 1** — đắt hơn 3-20x, thêm vendor risk, không cần thiết.

### Bảng giá dịch vụ (giai đoạn 1)

> **1 job = 5 variations**. Khách nhận 5 file MP4, không phải 1.

```
Gói 1 job (5 videos):         199k-250k VND   (~$8-10)    ← thử
Gói 3 job (15 videos):        500k VND        (~$20)      ← 15% discount
Gói 10 job (50 videos):       1.5tr VND       (~$60)      ← 25% discount
Gói tháng (10 job/tháng):     1.8tr VND       (~$72)      ← best value
Gói agency (no cap):          6tr VND/tháng   (~$240)     ← cho content studio
```

**Lý do pricing này hợp lý:**
- Seller đang trả 500k-2tr để thuê người quay + edit 1 video
- Bạn charge 199k cho **5 video** → khách dễ thử
- Sau khi có testimonial → tăng giá lên

### Kênh bán (không cần chạy ads, không tốn tiền)

**Tuần 1-2 (miễn phí hoàn toàn):**
1. Post trong Facebook Groups: "Kinh doanh Online VN", "Dropship Việt Nam", "TikTok Shop Seller VN"
   - Template post: "Mình đang test tool AI tạo 5 biến thể video quảng cáo từ 1 clip điện thoại.
     Bạn quay 30 giây thô → mình trả về 5 video khác hook để test TikTok Ads, miễn phí.
     [Đính kèm video before/after thực tế]. Ai muốn thử DM mình."
   - Goal: 10 người thử → 5 feedback → 2-3 người trả tiền

2. Nhắn tin trực tiếp seller đang chạy ads (thấy ads trên TikTok → tìm họ trên Facebook)
   - "Mình thấy bạn đang chạy ads sản phẩm X, mình có thể biến clip điện thoại của bạn
     thành 5 video ads khác hook trong 10 phút, free để test."

3. Post trên LinkedIn: "Mình vừa build tool AI tạo 5 biến thể UGC ads cho TikTok Shop"

**Tuần 3-4 (có testimonial rồi):**
- Post case study: "Trước/sau: seller test 5 variations → tìm ra hook thắng → CTR tăng X%"
- Offer gói tháng cho khách đã thử

### Tính toán thu nhập thực tế

**API cost per job (5 variations, Flow D):**
```
GPT-4o (analyze + 5 hooks + script):  $0.015
ElevenLabs (30-45s audio):             $0.030
FFmpeg (tự host):                      $0
Cloudflare R2 (storage 5 files):       $0.002
─────────────────────────────────────────────
Tổng cost/job (5 videos):             ~$0.047  (~1.2k VND)
```

**Nếu charge 200k VND/job (5 videos):**
- Cost 1.2k → Gross profit 198.8k VND (**99.4% margin**)
- **5 job/ngày = 1tr VND/ngày = ~30tr VND/tháng**

**Thực tế conservative hơn (3 job/ngày lúc đầu):**
- 3 job/ngày × 200k = 600k/ngày = 18tr/tháng
- Trừ API cost (~35k/tháng): **~17.9tr VND/tháng thuần**

**Khi scale lên gói tháng:**
- 10 khách × gói tháng 10 job = 100 job/tháng
- Revenue: 10 × 1.8tr = 18tr VND
- API cost: 100 × 1.2k = 120k VND
- **Net: ~17.9tr VND/tháng**

---

## PHẦN 4 — Giai Đoạn 2: Productized Service (Tháng 3-4)

Khi đã có 5-10 khách trả tiền đều đặn, scale bằng cách:

### Tạo "gói sản phẩm" rõ ràng

```
Starter Pack:    5 job/tháng  - 900k VND    (5 variations mỗi job)
Growth Pack:     15 job/tháng - 2.4tr VND   (+ ưu tiên xử lý)
Scale Pack:      50 job/tháng - 7tr VND     (cho agency)
```

### Automation thêm để tăng throughput

- Tạo form Google Form để khách nhập thông tin sản phẩm + upload video/ảnh
- Tool nhận form submission → tự chạy pipeline → gửi 5 MP4 về email
- Không cần tương tác tay với từng khách
- Bạn review quality → approve → send

### Thêm tính năng A/B có thể đo lường (V2)

Khi có 10+ khách trả tiền:
- Seller báo lại "V3 thắng" → system ghi nhận hook nào hiệu quả theo ngành
- Dần dần build được "hook library" theo ngành (thời trang, mỹ phẩm, gia dụng, thực phẩm)
- Hook library làm cho output ngày càng tốt hơn mà không tốn thêm chi phí AI

### Mở rộng sang SEA (tháng 4)

- Indonesia: TikTok Shop lớn nhất SEA, seller cần video tiếng Indonesia
- ElevenLabs có voice tiếng Indonesia/Malaysia → chỉ cần thêm ngôn ngữ vào pipeline
- Price tương đương hoặc cao hơn VN

---

## PHẦN 5 — Giai Đoạn 3: Chuyển Thành SaaS (Tháng 5+)

**Chỉ làm bước này khi:**
- [ ] Đã có ít nhất 20 khách trả tiền đều đặn
- [ ] Bạn biết đúng workflow khách cần (vì đã làm tay nhiều lần)
- [ ] Revenue từ service đủ pay cho infrastructure SaaS

### Thêm gì để biến internal tool thành SaaS

```
Thêm vào:
├── Auth (Clerk - $25/tháng, không tự build)
├── Billing (Stripe)
├── Self-serve UI (user tự upload, tự chọn voice)
├── Dashboard: lịch sử jobs + download 5 MP4
└── Usage limits (free/starter/pro/agency)

Giữ nguyên:
├── Content Intelligence (GPT-4o hooks/CTA/script)
├── Voice synthesis (ElevenLabs)
├── FFmpeg variation pipeline
└── Cloudflare R2 storage
```

**Timeline add SaaS layer: ~3-4 tuần (không phải 3-4 tháng)**
Vì core pipeline đã xây xong từ giai đoạn 1.

### SaaS Pricing

```
Free:     1 job/tháng — 5 variations (không cần thẻ)
Starter:  $19/tháng  — 10 job (50 videos)
Pro:      $49/tháng  — 30 job (150 videos) + campaign tracking
Agency:   $149/tháng — 100 job (500 videos) + white-label
```

### Roadmap tính năng sau SaaS (chỉ build khi có điều kiện)

```
V1  (Ngay)         Upload → 5 variations → Ship, charge 199k-250k/job
V2  (10 khách)     Controlled A/B tracking: seller báo lại kết quả
V3  (30 khách)     Hook library theo ngành từ data thật
V4  (100 khách)    Ad Strategy Engine — có data thật để strategy có ý nghĩa
V5  (doanh thu ổn) Competitor Intelligence — link TikTok đối thủ → phân tích
V6  (Phase 3)      Virtual Try-On (Kling) — chỉ fashion, lúc này mới đủ lý do
V7  (nếu cần)      Product Animation — chỉ build nếu AI video đủ ổn định
```

> V4 trở đi cần data thật. Không có user = không có data = chỉ là GPT hallucination.

---

## PHẦN 6 — Tech Stack (Tối Ưu Cho Vốn Ít)

### Giai đoạn 1-2 (Internal Tool): Giữ cực kỳ đơn giản

```
Backend:    ASP.NET Core 9 (giữ nguyên)
Database:   PostgreSQL (5 bảng — xem bên dưới)
AI:         GPT-4o (hooks/CTA/script) + Voice TBD (xem ghi chú bên dưới)
Video:      FFmpeg — core engine, tự host
Storage:    Cloudflare R2 (rẻ hơn S3, không cần self-host)
Queue:      Hangfire, WorkerCount=1 lúc đầu (tránh FFmpeg spike CPU trên Railway)
UI:         Razor Pages đơn giản (không cần Next.js lúc đầu)
Deploy:     Railway ($5/tháng hobby plan) — API + Worker cùng 1 instance lúc đầu
```

**⚠️ Voice provider — phải test trước khi code (Ngày 0, bắt buộc):**

| Provider | Điểm mạnh | Rủi ro |
|----------|----------|--------|
| FPT AI   | Train riêng tiếng Việt, giọng tự nhiên nhất, có Nam/Nữ Bắc/Nam | Doc ít hơn |
| Zalo AI  | Miễn phí tier cao | API kém ổn định |
| ElevenLabs | SDK tốt, tích hợp dễ | Tiếng Việt chưa chắc tự nhiên |

Test bằng cách: ghi cùng 1 script → cho 5-10 người nghe → chọn provider tốt nhất.
`IVoiceSynthesisService` đã abstract sẵn → swap vendor không phá code.

**Kế hoạch scale Railway khi cần (không cần build ngay):**
- Hiện tại: API + Hangfire Worker cùng 1 Railway instance, `WorkerCount=1`
- Khi có >10 job/ngày: tách Hangfire Worker ra instance riêng, giữ nguyên code

**Deprioritized (giữ code, không build thêm):**
```
HeyGenAvatarVideoService   ← Flow A, không dùng trong MVP
KlingService               ← Flow B/C, dùng sau V6 nếu cần
ShopeeProductScraper       ← để V4+
ProcessFlowA/B/CCommandHandler ← không build thêm
```

### Chi phí hàng tháng (giai đoạn 1)

```
Railway (deploy):          $5/tháng
Cloudflare R2 (storage):   ~$1-3/tháng (pay per use)
Domain (.com):             $10/năm (~$0.8/tháng)
─────────────────────────────────────────
Infrastructure:            ~$7/tháng cố định

API costs (variable, 50 job/tháng = 250 videos):
  - ElevenLabs Starter:  $5/tháng (30k chars)
  - GPT-4o:              ~$0.015/job → 50 job = $0.75
─────────────────────────────────────────
Tổng fixed + variable (50 job/tháng):  ~$13/tháng
```

> Khi bắt đầu, mọi API cost đều là variable — bạn chỉ trả khi có job.
> Không có job = không tốn tiền.

### Schema Database (5 bảng, đủ cho giai đoạn 1-3)

```sql
users          -- id, email, name
video_jobs     -- id, user_id, product_description, input_type (video/images),
               -- input_url, status, created_at
job_briefs     -- id, job_id, hooks_json, ctas_json, voice_script, voice_url
job_variations -- id, job_id, variation_index, hook_text, cta_text,
               -- output_url, status
api_costs      -- id, job_id, service, cost_usd, created_at
```

---

## PHẦN 7 — Core Flow Kỹ Thuật (Cụ Thể)

### Pipeline 1 job → 5 variations (end-to-end)

```
1. INPUT
   - Seller upload 1 video (30-60s) hoặc 3-5 ảnh sản phẩm
   - Nhập: tên sản phẩm + giá (để CTA overlay)

2. CONTENT INTELLIGENCE (GPT-4o)
   - Nếu video: FFmpeg extract 3 frames → GPT-4o Vision phân tích
   - Nếu ảnh: GPT-4o Vision phân tích trực tiếp
   - Output: { hooks[5], ctas[3], voiceScript }

3. VOICE SYNTHESIS (ElevenLabs)
   - ElevenLabs.TextToSpeech(voiceScript, voice_id) → voiceover.mp3
   - Giữ timestamps để sync caption

4. BASE COMPOSITION (FFmpeg)
   - Input: raw video/slideshow ảnh + voiceover + background music
   - Xử lý: color grade (vibrant + warm) → mix audio → burn-in captions
            → film grain nhẹ → scale/crop → 9:16 1080×1920
   - Output: base_video.mp4

5. VARIATION LOOP (FFmpeg, chạy song song)
   - base_video × hooks[0] → variation_0.mp4  (text overlay đầu video)
   - base_video × hooks[1] → variation_1.mp4
   - base_video × hooks[2] → variation_2.mp4
   - base_video × hooks[3] → variation_3.mp4
   - base_video × hooks[4] → variation_4.mp4

6. UPLOAD + DELIVER
   - Upload 5 files → Cloudflare R2 → 5 presigned URLs
   - Gửi links về cho khách (email hoặc trong dashboard)
```

### FFmpeg command mẫu (base composition)

```bash
ffmpeg -y \
  -i raw_footage.mp4 \
  -i voiceover.mp3 \
  -i background_music.mp3 \
  -filter_complex "
    [0:v]scale=1080:1920:force_original_aspect_ratio=increase,
         crop=1080:1920,eq=brightness=0.05:saturation=1.3:contrast=1.1[v];
    [v]subtitles=captions.srt:force_style='FontSize=18,PrimaryColour=&HFFFFFF'[vout];
    [2:a]volume=0.15[music];
    [1:a][music]amix=inputs=2:duration=first[aout]
  " \
  -map "[vout]" -map "[aout]" \
  -c:v libx264 -c:a aac \
  base_video.mp4
```

### IContentIntelligenceService (interface mới)

```csharp
// src/Application/Common/Interfaces/IContentIntelligenceService.cs
public interface IContentIntelligenceService
{
    Task<ContentBrief> GenerateBriefAsync(string productDescription, string inputType);
}

public record ContentBrief(
    string[] Hooks,      // 5 hooks phù hợp nhất với sản phẩm — GPT tự chọn angle
    string[] Ctas,       // 3 CTA options
    string VoiceScript   // 30-45s voiceover script
);
```

---

## PHẦN 8 — Kế Hoạch Build 7 Ngày

### Mục tiêu cuối ngày 7: Tool chạy end-to-end, xuất được 5 MP4

| Ngày | Việc làm | File liên quan |
|------|----------|----------------|
| **1** | Upload endpoint + Hangfire wiring + R2 storage | `CreateJobCommand`, `CloudflareR2StorageService` |
| **2** | `IContentIntelligenceService` + GPT prompt cho hooks/CTA/script | `OpenAiScriptGeneratorService` (mở rộng) |
| **3** | Voice synthesis từ script → mp3 + caption timestamps | `ElevenLabsVoiceSynthesisService` |
| **4** | FFmpeg base composition: video/ảnh + voice + caption + nhạc | `FfmpegVideoComposerService` (thêm `ComposeBaseAsync`) |
| **5** | Variation loop: base × 5 hooks → 5 MP4 | `ProcessFlowDCommandHandler` |
| **6** | UI: upload → progress polling → download 5 files | Razor Pages |
| **7** | Deploy Railway + test end-to-end với sản phẩm thật | — |

### Tuần 3-4: Ra Tiền

```
Ngày 8-9:   Tạo landing page (Notion/Framer free) + order form (Google Form)
Ngày 10-11: Post vào 3 Facebook Groups → offer 1 job free cho đầu tiên inbox
Ngày 12-14: Làm 3-5 job mẫu với sản phẩm thật để show portfolio
Ngày 15+:   → Nhận order đầu tiên trả tiền → iterate dựa trên feedback thật
```

---

## PHẦN 9 — Milestone & Revenue Target

> **Lưu ý**: "5 job/ngày = 30tr/tháng" đúng về toán học nhưng sai về thực tế.
> Thách thức không phải làm video — thách thức là có đủ khách đặt job liên tục.
> Dùng milestone theo số khách, không phải số job/ngày.

| Milestone | Mục tiêu | Revenue thực tế |
|-----------|---------|---------|
| Ngày 7    | Tool chạy end-to-end, xuất 5 MP4 | $0 |
| Tuần 3    | 3 job free → feedback thật | $0 |
| Tuần 4    | 2-3 paying customers đầu tiên | ~400-600k VND |
| **Tháng 1** | **0-5 khách** — validate product fit | ~1-2tr VND |
| **Tháng 2** | **5-10 khách** — tìm kênh bán ổn định | ~3-5tr VND/tháng |
| **Tháng 3** | **10-20 khách** — gói tháng bắt đầu, predictable revenue | ~8-12tr VND/tháng |
| Tháng 4   | SEA expansion, gói agency | ~15-20tr VND/tháng |
| Tháng 5+  | Bắt đầu SaaS layer khi có 20+ khách | Service + SaaS |

---

## PHẦN 10 — Rủi Ro Thực Tế & Cách Xử Lý

| Rủi ro | Xác suất | Cách xử lý |
|--------|---------|-----------|
| Seller không chịu quay raw footage | Trung bình | Hỗ trợ mode ảnh (slideshow) → không cần video thô |
| Caption sync bị lệch | Trung bình | Dùng ElevenLabs timestamps API hoặc tự tính từ word count |
| Kling/HeyGen thay đổi giá | Thấp | Không dùng trong MVP — không bị ảnh hưởng |
| Khách không chịu trả tiền | Thấp | Charge từ job thứ 2 trở đi, job đầu free |
| Không có khách | Thấp | Direct message seller đang chạy ads — targeted hơn |

---

## PHẦN 11 — Quyết Định Cuối Cùng & Action Items

### Giữ lại từ project cũ

- ASP.NET Core 9 + Clean Architecture ✓
- EF Core + PostgreSQL ✓
- ElevenLabs integration ✓
- FFmpeg pipeline ✓
- Cloudflare R2 + Hangfire ✓

### Bỏ khỏi MVP (không cần cho giai đoạn 1-2)

- HeyGen Avatar — trông giả tạo, đắt, không fit TikTok Shop VN
- Kling Try-On / Image-to-Video — để V6+, khi revenue đủ absorb risk
- Shopee Scraper — seller tự upload, không cần scrape
- RabbitMQ → Hangfire (đã làm)
- MinIO → Cloudflare R2 (đã làm)
- Redis → không cần
- 19-bảng schema → 5 bảng

### Action items ngay bây giờ (trước khi code)

- [ ] **Ngày 0 (bắt buộc trước khi code)**: Test voice tiếng Việt — chạy cùng 1 script qua FPT AI + Zalo AI + ElevenLabs → cho 5-10 người nghe → chọn provider
- [ ] **Ngày 0**: Quay 2-3 video thô bằng điện thoại với sản phẩm thật → test data
- [ ] **Ngày 1**: Build upload endpoint + wiring Hangfire (WorkerCount=1)
- [ ] **Ngày 2**: Viết GPT prompt sinh 5 hooks × 3 CTAs × 1 script
- [ ] **Ngày 4**: FFmpeg base composition pipeline → so sánh before/after
- [ ] **Ngày 5**: `ProcessFlowDCommandHandler` end-to-end

### Nguyên tắc không được phá vỡ

> **Bán kết quả trước, bán tool sau.**
> 5 variations để test > 1 video đẹp để xem.
> "Anh/chị sẽ biết hook nào convert tốt nhất" là pitch mạnh hơn "video AI đẹp lắm".
> Đừng build SaaS trước khi có 20 người trả tiền cho dịch vụ của bạn.
> Đừng build feature nào nếu chưa có user thật cần nó.
