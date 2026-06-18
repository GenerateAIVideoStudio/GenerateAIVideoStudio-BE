# Các Quyết Định Thiết Kế Quan Trọng
> Cập nhật: 2026-06-18
> Ghi lại WHY đằng sau các quyết định trong codebase — tránh quay lại tranh luận cũ.

---

## 1. GPT Không Hard-Code Hook Types

**Quyết định**: GPT tự chọn angle phù hợp nhất cho từng sản phẩm, không pre-define 5 loại.

**Tại sao**:
- Máy hút bụi → pain angle rất mạnh
- Nước hoa → pain gần như vô nghĩa, desire/identity quan trọng hơn
- Hard-code 5 types → output generic, không phù hợp từng sản phẩm

**Cách làm**: Prompt yêu cầu GPT đánh giá và chọn top 5 angles thay vì điền vào template cố định.
Xem prompt tại: [04-ai-apis-and-infrastructure.md § ContentIntelligence](04-ai-apis-and-infrastructure.md)

---

## 2. Voice Provider: Test Ngày 0, Không Code Mù

**Quyết định**: Bắt buộc test thật (FPT AI vs Zalo AI vs ElevenLabs) trước khi code, với người nghe thật.

**Tại sao**:
- Nếu code 7 ngày xong phát hiện voice nghe như robot → tốn thời gian sửa
- ElevenLabs `eleven_multilingual_v2` tốt cho English, nhưng tiếng Việt chưa chắc
- FPT AI train riêng cho tiếng Việt → khả năng cao tự nhiên hơn

**Cách làm**: `IVoiceSynthesisService` đã abstract → chỉ đổi implementation trong DI, không phá pipeline.

---

## 3. Railway WorkerCount=1

**Quyết định**: Hangfire `WorkerCount=1` cho toàn bộ Giai đoạn 1-2.

**Tại sao**:
- FFmpeg encode video tốn CPU nặng
- 5 jobs cùng lúc × FFmpeg × Railway Hobby = OOM hoặc timeout
- `WorkerCount=1` → jobs queue lần lượt → ổn định hơn

**Khi nào scale**: Khi > 10 job/ngày → tách Hangfire Worker ra Railway instance riêng.
Không cần thay đổi code — chỉ thay đổi infrastructure.

---

## 4. Milestone Theo Số Khách, Không Theo Job/Ngày

**Quyết định**: Revenue target đặt theo số khách, không phải "5 job/ngày = 30tr/tháng".

**Tại sao**:
- "5 job/ngày" đúng về toán học nhưng sai về thực tế
- Thách thức THẬT là có đủ khách đặt job liên tục — không phải làm video
- Milestone theo số khách thực tế hơn và giúp tập trung vào đúng metric

**Timeline thực tế**:
```
Tháng 1: 0-5 khách   → validate product fit
Tháng 2: 5-10 khách  → tìm kênh bán ổn định
Tháng 3: 10-20 khách → gói tháng, predictable revenue
```

---

## 5. Không Dùng Kling/HeyGen Trong MVP

**Quyết định**: MVP chỉ dùng GPT-4o + ElevenLabs/FPT AI + FFmpeg.

**Tại sao**:
- Kling: $0.035/ảnh try-on, $0.14/video 5s → đắt hơn 3-20x
- HeyGen: $0.40-1.00/video avatar → margin thấp, trông giả tạo
- FFmpeg: $0/video, tự host, không vendor risk
- MVP cost với FFmpeg: $0.047/job (5 videos) vs $0.40-1.00/video với Kling/HeyGen

**Khi nào dùng Kling**: V6+ (sau 20 paying customers, fashion sellers cụ thể yêu cầu try-on).

---

## 6. Base Video + Variation Architecture

**Quyết định**: Tách pipeline thành "compose base" và "compose variation" riêng biệt.

**Tại sao**:
- Nếu compose cả base+hook trong 1 bước → phải encode 5 lần hoàn toàn (chậm)
- Với architecture hiện tại:
  - Encode base 1 lần (chậm, nặng)
  - Add text overlay 5 lần (`-c:a copy` → chỉ re-encode video, không audio)
  - 5 variations chạy song song → tổng thời gian ≈ thời gian 1 variation

---

## 7. Schema: job_briefs Table Riêng

**Quyết định**: Lưu hooks/CTAs/script trong bảng `job_briefs` riêng, không lưu trong `video_jobs`.

**Tại sao**:
- `video_jobs` chỉ chứa metadata (status, input, email) → đơn giản, dễ query
- `job_briefs` chứa output của GPT → 1-1 với VideoJob → có thể null khi scripting chưa xong
- Tách ra giúp UI hiển thị hooks/script ngay khi scripting xong (trước khi video render)

---

## 8. Roadmap Data-First

**Quyết định**: Chỉ build feature khi có user thật cần nó và có data để support.

```
V4 (Ad Strategy Engine) cần data thật để có ý nghĩa
  → Không có user = không có data = chỉ là GPT hallucination

V5 (Competitor Intelligence) cần feedback loop
  → Không có user = không có vòng lặp học

V6, V7 phụ thuộc AI vendor tốn kém
  → Build sau khi revenue đủ để absorb risk
```

**Câu hỏi duy nhất trước khi build feature mới**:
> "Tính năng này giúp mình giao được 5 videos cho 20 sellers nhanh hơn không?"
> Nếu không → không build bây giờ.
