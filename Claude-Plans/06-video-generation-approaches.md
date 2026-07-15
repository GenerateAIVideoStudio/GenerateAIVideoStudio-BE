# Video Generation — Creative Variation Engine
> Cập nhật: 2026-06-18
> MVP: 1 approach duy nhất — Raw Footage / Ảnh → 5 Variations với hook khác nhau.
> Kling/HeyGen deprioritized đến V6+ (sau 20 paying customers).

---

## CORE APPROACH — Creative Variation Engine

```
Mục tiêu không phải "1 video đẹp".
Mục tiêu là: "5 video để test xem hook nào convert tốt nhất".

Seller không mua video AI.
Seller mua khả năng tìm ra góc quảng cáo thắng.
```

### Controlled Variation — Thay 1 Biến Mỗi Lần

```
V1: Hook A + CTA gốc + Voice gốc          ← baseline
V2: Hook B + CTA gốc + Voice gốc          ← chỉ đổi hook
V3: Hook C + CTA gốc + Voice gốc          ← chỉ đổi hook
V4: Hook D + CTA gốc + Voice gốc          ← chỉ đổi hook
V5: Hook E + CTA gốc + Voice gốc          ← chỉ đổi hook
```

Seller upload lên TikTok Ads → sau 2 ngày biết hook nào CTR cao nhất → nhân bản hook đó.

---

## PIPELINE 4 BƯỚC

```
INPUT
  Seller upload: 1 video (30-60s) hoặc 3-5 ảnh sản phẩm
  Seller nhập: mô tả sản phẩm + giá
       ↓
STEP 1 — Content Intelligence (GPT-4o) ~ 10 giây
  Phân tích sản phẩm → sinh:
    hooks[]   : 5 hooks phù hợp nhất (GPT tự chọn angle, không hard-code)
    ctas[]    : 3 CTA options
    script    : voiceover script 30-45s
       ↓
STEP 2 — Voice Synthesis (ElevenLabs/FPT AI) ~ 5 giây
  script → voiceover.mp3 → upload R2
       ↓
STEP 3 — Base Composition (FFmpeg) ~ 30-60 giây
  Input: raw video hoặc slideshow ảnh
  Processing:
    - Scale/crop → 9:16 1080×1920
    - Color grade: saturation+30%, brightness+5%, contrast+10%
    - Mix voiceover overlay
    - Burn-in captions (sync với voiceover)
    - Background music trending (volume 15%, fade out)
    - Film grain nhẹ (trông "thật" hơn)
  Output: base_video.mp4 (KHÔNG có hook text — thêm ở bước sau)
       ↓
STEP 4 — Variation Generator (FFmpeg × 5, song song) ~ 30 giây tổng
  base_video + hooks[0] → variation_0.mp4  (text overlay 3s đầu)
  base_video + hooks[1] → variation_1.mp4
  base_video + hooks[2] → variation_2.mp4
  base_video + hooks[3] → variation_3.mp4
  base_video + hooks[4] → variation_4.mp4
       ↓
OUTPUT
  5 file MP4 sẵn sàng upload TikTok Ads / Meta Ads
  Total time: ~ 5-10 phút
  Cost: ~$0.047 per job
```

---

## CHI TIẾT BƯỚC 1: CONTENT INTELLIGENCE

### Cách GPT chọn hooks

GPT KHÔNG được hard-code 5 loại hook. Thay vào đó:

```
1. Phân tích loại sản phẩm
2. Đánh giá độ phù hợp của từng angle với sản phẩm đó
3. Chọn top 5 angles và viết hooks cho chúng

Ví dụ — Máy hút bụi mini:
  → Pain (nhà bẩn, nỗi đau dọn dẹp): score cao
  → Curiosity (cơ chế hoạt động lạ): score cao
  → Convenience (tiết kiệm thời gian): score trung bình
  → Social proof (nhiều người đang dùng): score thấp
  → Comparison (vs. máy hút bụi to): score trung bình

Ví dụ — Nước hoa nữ:
  → Pain: score thấp (nước hoa không giải quyết "nỗi đau")
  → Desire/identity (bạn là ai khi đeo cái này): score rất cao
  → Transformation (từ bình thường → quyến rũ): score cao
  → Social proof (ai đang dùng): score cao
  → Sensory (mùi hương gợi cảm): score cao
```

### Prompt cho GPT (tham khảo)

```
Xem implementation đầy đủ tại: 04-ai-apis-and-infrastructure.md § ContentIntelligence

Điểm quan trọng trong prompt:
- Không list 5 angles cố định → GPT tự quyết định
- Viết hooks ngắn (3-8 từ), gây tò mò hoặc đánh trúng pain/desire
- VoiceScript phải đọc tự nhiên như người thật, không giống quảng cáo
- Output: JSON thuần { hooks[], ctas[], voice_script }
```

---

## CHI TIẾT BƯỚC 3: BASE VIDEO COMPOSITION

### Trường Hợp 1: Input Là Video (Raw Footage)

```
Seller quay 30-60s unboxing/review bằng điện thoại
→ Trim 1s đầu + cuối (thường run tay)
→ Color grade: TikTok-style vibrant + slightly warm
→ Mix voiceover (audio gốc giảm 20% hoặc tắt hoàn toàn nếu ồn)
→ Burn-in captions sync với voiceover
→ Background music nhẹ (volume 15%)
→ Film grain nhẹ (trông "real phone-shot")
→ Scale/crop 9:16

Kết quả: video trông professional hơn nhưng vẫn có cảm giác authentic
```

**Audio strategy**:
```
Option A (khuyến khích): Mix gốc + voiceover
  → Giữ ambient sound gốc
  → Voiceover overlay (volume 80%)
  → Background music (volume 15%)

Option B: Replace hoàn toàn (khi audio gốc quá ồn)
  → Tắt audio gốc
  → Chỉ voiceover + music
```

### Trường Hợp 2: Input Là Ảnh (Slideshow)

```
Seller upload 3-5 ảnh sản phẩm
→ Slideshow với Ken Burns effect (pan/zoom nhẹ)
→ xfade fade transition giữa ảnh
→ Voiceover chạy xuyên suốt
→ Captions sync

Phù hợp cho seller không có raw footage.
Kết quả đơn giản hơn video thật nhưng vẫn đủ tốt để test ads.
```

---

## CHI TIẾT BƯỚC 4: VARIATION GENERATION

### Logic Variation

```
base_video.mp4 (không có hook text)
        ↓ × 5 (song song)
variation_N.mp4 (thêm hook text overlay vào 3s đầu)
```

### Hook Text Overlay Spec

```
Position: trên (y = 15% from top)
Font size: 36px
Color: white text + black semi-transparent background
Duration: 0 → 3 giây
Style: box with padding, rounded
```

### Tại Sao Song Song?

```
5 variations chạy song song với Task.WhenAll:
- Mỗi variation chỉ cần add text overlay → rất nhanh (không re-encode nặng)
- -c:a copy: không re-encode audio
- Tổng thời gian = thời gian 1 variation (không phải 5x)
- Kết quả: 5 files MP4 trong ~30 giây
```

---

## SO SÁNH: BASE VIDEO vs VARIATION

| | Base Video | Variation |
|--|--|--|
| Hook text overlay | KHÔNG (base chỉ có caption voiceover) | CÓ (đầu 3 giây) |
| CTA overlay | CÓ (cuối video) | CÓ (từ base) |
| Voice | CÓ (từ base) | Giữ nguyên |
| Music | CÓ (từ base) | Giữ nguyên |
| Lưu trữ R2 | Có (tạm thời) | Có (permanent) |
| Giao cho khách | KHÔNG | CÓ (5 files) |

---

## FLOWS TƯƠNG LAI (V6+)

Chỉ build sau khi có 20+ paying customers và revenue ổn định.

### V6 — Virtual Try-On (Kling) — Fashion Only

```
Khi nào build:
- 20+ paying customers, trong đó có nhiều fashion seller
- Revenue đủ để absorb Kling pricing ($0.035/ảnh try-on)

Pipeline thêm vào:
Seller upload ảnh sản phẩm (áo/váy/giày)
→ Kling kolors-virtual-try-on-v1-5: ảnh model + ảnh sản phẩm → try-on image
→ 3 models song song → 3 ảnh try-on
→ Slideshow 3 ảnh + voiceover + captions
→ Variation generator × 5

Fallback: nếu Kling distort → Fashn.ai (chuyên fashion hơn)
```

### V7 — Product Animation (Kling Image-to-Video) — Beauty/Jewelry

```
Khi nào build:
- Sau V6, khi đã có kinh nghiệm với Kling API
- Beauty/jewelry sellers phản hồi muốn animation

Pipeline:
Ảnh sản phẩm nền trắng
→ Kling image-to-video: ảnh → 5s product demo clip
   (motion prompt theo ngành: beauty/skincare/jewelry/food)
→ Retry tối đa 3 lần nếu distort
→ Fallback: slideshow nếu tất cả retry fail
→ Compose + Variation × 5

Motion prompts theo ngành:
beauty:   "product floats gently, cap opens, slow rotation, studio lighting"
skincare: "cream texture close-up, slow rotation, dewy finish"
jewelry:  "rotates 360 degrees, sparkle effect, dark background"
```

---

## PITCHING

### Với Khách Hàng

```
"Anh/chị chỉ cần quay 30 giây unboxing sản phẩm bằng điện thoại.
Không cần đẹp, không cần edit gì cả.
Em trả về 5 video khác hook trong 10 phút.
Anh/chị biết hook nào convert tốt nhất sau 2 ngày test trên TikTok Ads."
```

### Giá Trị Cụ Thể

```
Thuê UGC creator thật:   $50-300/video, mất 5-7 ngày
Tool này:                199k VND cho 5 videos, trong 10 phút

Không phải "video AI đẹp hơn".
Mà là "5 biến thể để test, nhanh, rẻ".
```

---

## LƯỚI DECISION KHI SELLER LIÊN HỆ

```
Seller có raw footage (video điện thoại)?
  → YES: Upload video → 5 variations (BEST CASE)
  → NO: Seller có ảnh sản phẩm?
         → YES: Upload ảnh → 5 variations slideshow
         → NO: Yêu cầu seller quay 30s với điện thoại
               (barrier thấp — 90% đồng ý khi thấy before/after demo)
```
