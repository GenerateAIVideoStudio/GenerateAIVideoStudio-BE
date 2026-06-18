# Frontend Architecture
> Cập nhật: 2026-06-18
> Giai đoạn 1-2: Razor Pages (internal tool, ra nhanh, đủ dùng).
> Giai đoạn 3: Next.js (khi làm SaaS cho user tự dùng — chỉ sau khi có 20 paying customers).

---

## Giai Đoạn 1-2: Razor Pages (Internal Tool)

### Tại sao Razor Pages, không phải React/Next.js?
- Cùng project với backend ASP.NET — không cần 2 server riêng
- Không cần setup CORS, JWT, API client
- Đủ để tự dùng và demo cho khách
- Chuyển sang Next.js sau khi validate product

### Cấu trúc Pages

```
src/Api/Pages/
├── _Layout.cshtml
├── Index.cshtml             ← redirect → /Jobs
│
├── Jobs/
│   ├── Index.cshtml         ← danh sách jobs + filter status
│   ├── Index.cshtml.cs
│   ├── New.cshtml           ← form upload + mô tả sản phẩm
│   ├── New.cshtml.cs
│   └── Detail/
│       ├── {id}.cshtml      ← progress 4 bước + 5 video player + download
│       └── {id}.cshtml.cs
│
└── Dashboard/
    └── Index.cshtml         ← cost tracking tổng ngày/tháng
```

---

### Jobs/Index — Danh Sách Jobs

```csharp
// Index.cshtml.cs
public class IndexModel : PageModel
{
    public List<VideoJobDto> Jobs { get; set; } = [];
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }

    public async Task OnGetAsync()
    {
        Jobs = await _mediator.Send(new GetJobListQuery(Status, Page: 1));
    }
}
```

```html
<!-- Index.cshtml -->
@page
@model IndexModel
<h1>Video Jobs</h1>
<a href="/Jobs/New">+ Tạo Job Mới</a>

<!-- Filter tabs -->
<nav>
  <a href="?status=">Tất cả (@Model.Jobs.Count)</a>
  <a href="?status=pending">⏳ Pending</a>
  <a href="?status=done">✅ Done</a>
  <a href="?status=error">❌ Error</a>
</nav>

<table>
  <thead>
    <tr>
      <th>Thời gian</th>
      <th>Mô tả sản phẩm</th>
      <th>Loại input</th>
      <th>Trạng thái</th>
      <th>Cost</th>
      <th></th>
    </tr>
  </thead>
  <tbody>
    @foreach (var job in Model.Jobs)
    {
      <tr>
        <td>@job.CreatedAt.ToString("dd/MM HH:mm")</td>
        <td>@(job.ProductDescription?.Length > 50
              ? job.ProductDescription[..50] + "..."
              : job.ProductDescription)</td>
        <td>@(job.InputType == "video" ? "📹 Video" : "🖼️ Ảnh")</td>
        <td><span class="badge status-@job.Status">@job.Status</span></td>
        <td>$@job.TotalCostUsd.ToString("F4")</td>
        <td><a href="/Jobs/Detail/@job.Id">Xem</a></td>
      </tr>
    }
  </tbody>
</table>

<!-- Auto-refresh khi có job đang chạy -->
@if (Model.Jobs.Any(j => j.Status is not "done" and not "error"))
{
  <script>setTimeout(() => location.reload(), 5000)</script>
}
```

---

### Jobs/New — Form Tạo Job

```html
<!-- New.cshtml -->
@page
@model NewModel
<h1>Tạo Video Quảng Cáo Mới</h1>
<p>Upload 1 video hoặc 3-5 ảnh sản phẩm → nhận <strong>5 video quảng cáo khác hook</strong> trong 10 phút.</p>

<form method="post" enctype="multipart/form-data">
  <!-- Upload file -->
  <section>
    <h2>1. Upload video hoặc ảnh</h2>
    <input type="file" name="InputFile"
           accept="video/mp4,video/quicktime,image/jpeg,image/png,image/webp"
           required />
    <small>
      📹 Video: quay 30-60s sản phẩm bằng điện thoại, không cần đẹp<br/>
      🖼️ Ảnh: upload 1-5 ảnh sản phẩm (hỗ trợ JPG/PNG)
    </small>
  </section>

  <!-- Mô tả sản phẩm -->
  <section>
    <h2>2. Mô tả sản phẩm</h2>
    <textarea asp-for="ProductDescription" rows="3"
              placeholder="VD: Máy hút bụi mini không dây, giá 299k, pin 2 tiếng, có thể hút xe hơi và bàn phím"
              required></textarea>
    <small>Ghi rõ: tên sản phẩm + giá + điểm nổi bật. GPT sẽ dùng thông tin này để viết hook.</small>
  </section>

  <!-- Chọn giọng -->
  <section>
    <h2>3. Giọng đọc</h2>
    <select asp-for="VoiceId">
      @foreach (var v in Model.AvailableVoices)
      { <option value="@v.Id">@v.Name — @v.Accent</option> }
    </select>
  </section>

  <!-- Email giao hàng -->
  <section>
    <h2>4. Email nhận file (tùy chọn)</h2>
    <input asp-for="CustomerEmail" type="email"
           placeholder="seller@email.com" />
    <textarea asp-for="Notes" rows="2"
              placeholder="Ghi chú thêm cho AI: tone thương hiệu, từ không dùng..."></textarea>
  </section>

  <button type="submit">Tạo 5 Video →</button>
</form>
```

```csharp
// New.cshtml.cs
public class NewModel : PageModel
{
    [BindProperty] public IFormFile? InputFile { get; set; }
    [BindProperty] public string? ProductDescription { get; set; }
    [BindProperty] public string? VoiceId { get; set; }
    [BindProperty] public string? CustomerEmail { get; set; }
    [BindProperty] public string? Notes { get; set; }

    public List<VoiceOption> AvailableVoices { get; set; } = [];

    public async Task OnGetAsync()
    {
        AvailableVoices = await _voiceService.GetAvailableVoicesAsync("vi");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (InputFile is null) return Page();

        // Upload file lên R2 trước
        var ext = Path.GetExtension(InputFile.FileName).ToLower();
        var inputType = ext is ".mp4" or ".mov" or ".avi"
            ? InputType.Video : InputType.Images;
        var objectKey = $"inputs/{Guid.NewGuid()}{ext}";

        await using var stream = InputFile.OpenReadStream();
        await _storage.UploadAsync(stream, objectKey, InputFile.ContentType);

        // Tạo job
        var cmd = new CreateJobCommand(
            ProductDescription : ProductDescription,
            InputType          : inputType,
            InputObjectKey     : objectKey,
            VoiceId            : VoiceId,
            CustomerEmail      : CustomerEmail,
            Notes              : Notes);

        var jobId = await _mediator.Send(cmd);

        // Enqueue Hangfire
        _hangfire.Enqueue<IMediator>(m =>
            m.Send(new ProcessFlowDCommand(jobId, VoiceId), CancellationToken.None));

        return RedirectToPage("/Jobs/Detail", new { id = jobId });
    }
}
```

---

### Jobs/Detail — Xem Kết Quả

```html
<!-- Detail/{id}.cshtml -->
@page "{id:guid}"
@model DetailModel

<h1>Job #@Model.Job.Id.ToString()[..8]</h1>
<p>@Model.Job.ProductDescription</p>

<!-- Progress bar 4 bước -->
@{
  var steps = new[] { "scripting", "voicing", "composing", "generating", "done" };
  var labels = new[] { "GPT Script", "Voice", "Base Video", "5 Variations", "Xong" };
}
<div class="progress-steps">
  @for (int i = 0; i < steps.Length; i++)
  {
    var isDone   = IsStepCompleted(Model.Job.Status, steps[i]);
    var isActive = Model.Job.Status == steps[i];
    <div class="step @(isDone ? "done" : "") @(isActive ? "active" : "")">
      @(isDone ? "✓" : (i + 1).ToString()) @labels[i]
    </div>
  }
</div>

<!-- Auto-refresh khi đang chạy -->
@if (Model.Job.Status is not "done" and not "error")
{
  <p><em>Đang xử lý... tự động cập nhật sau 5 giây</em></p>
  <script>setTimeout(() => location.reload(), 5000)</script>
}

<!-- Script preview (khi scripting xong) -->
@if (Model.Job.Brief != null)
{
  <details>
    <summary>Script & Hooks đã generate</summary>
    <p><strong>Voice Script:</strong> @Model.Job.Brief.VoiceScript</p>
    <p><strong>5 Hooks:</strong></p>
    <ol>
      @foreach (var hook in Model.Hooks)
      { <li>@hook</li> }
    </ol>
  </details>
}

<!-- 5 Videos (khi done) -->
@if (Model.Job.Status == "done" && Model.Variations.Any())
{
  <section>
    <h3>5 Video Quảng Cáo ✅</h3>
    <p><small>Mỗi video dùng 1 hook khác nhau. Upload lên TikTok Ads để test.</small></p>
    <div style="display:flex; flex-wrap:wrap; gap:16px">
      @foreach (var v in Model.Variations.OrderBy(x => x.VariationIndex))
      {
        var downloadUrl = await _storage.GetPresignedUrlAsync(v.OutputUrl!);
        <div style="border:1px solid #e5e7eb; border-radius:8px; padding:12px; width:200px">
          <p style="font-size:12px; font-weight:600">V@(v.VariationIndex + 1)</p>
          <p style="font-size:11px; color:#6b7280">@v.HookText</p>
          <video controls width="180" src="@downloadUrl"
                 style="border-radius:4px"></video>
          <br/>
          <a href="@downloadUrl" download="variation_@(v.VariationIndex + 1).mp4"
             style="font-size:12px">⬇ Download</a>
        </div>
      }
    </div>
  </section>
}

<!-- Error panel -->
@if (Model.Job.Status == "error")
{
  <div style="background:#fee2e2; padding:16px; border-radius:8px">
    <strong>Lỗi:</strong> @Model.Job.ErrorMessage
    <form method="post" asp-page-handler="Retry">
      <input type="hidden" name="id" value="@Model.Job.Id" />
      <button style="margin-top:8px">🔄 Thử lại</button>
    </form>
  </div>
}

<!-- API Cost breakdown -->
<details>
  <summary>Chi phí API ($@Model.Job.ApiCosts.Sum(c => c.CostUsd).ToString("F4") total)</summary>
  <table>
    <tr><th>Service</th><th>Action</th><th>Cost</th></tr>
    @foreach (var c in Model.Job.ApiCosts)
    { <tr><td>@c.Service</td><td>@c.Action</td><td>$@c.CostUsd.ToString("F5")</td></tr> }
  </table>
</details>
```

```csharp
// Detail/{id}.cshtml.cs
public class DetailModel : PageModel
{
    public VideoJobDto Job { get; set; } = null!;
    public List<string> Hooks { get; set; } = [];
    public List<JobVariationDto> Variations { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var dto = await _mediator.Send(new GetJobQuery(id));
        if (dto is null) return NotFound();

        Job = dto;
        Hooks = dto.Brief?.HooksJson is not null
            ? JsonSerializer.Deserialize<List<string>>(dto.Brief.HooksJson.RootElement.GetRawText()) ?? []
            : [];
        Variations = dto.Variations;
        return Page();
    }

    public async Task<IActionResult> OnPostRetryAsync(Guid id)
    {
        _hangfire.Enqueue<IMediator>(m =>
            m.Send(new ProcessFlowDCommand(id, null), CancellationToken.None));
        return RedirectToPage(new { id });
    }

    private static bool IsStepCompleted(string currentStatus, string step) => (currentStatus, step) switch
    {
        ("scripting",  "scripting")  => true,
        ("voicing",    "scripting")  => true,
        ("voicing",    "voicing")    => true,
        ("composing",  "scripting")  => true,
        ("composing",  "voicing")    => true,
        ("composing",  "composing")  => true,
        ("generating", var s) when s is "scripting" or "voicing" or "composing" => true,
        ("done",       var s) when s is not "done" => true,
        ("done",       "done")       => true,
        _ => false
    };
}
```

---

### Dashboard/Index — Cost Tracking

```html
@page
@model DashboardModel
<h1>Dashboard</h1>

<div style="display:flex; gap:24px; flex-wrap:wrap">
  <div style="border:1px solid #e5e7eb; border-radius:8px; padding:16px; min-width:160px">
    <div style="font-size:24px; font-weight:700">@Model.JobsToday</div>
    <div>Jobs hôm nay</div>
  </div>
  <div style="border:1px solid #e5e7eb; border-radius:8px; padding:16px; min-width:160px">
    <div style="font-size:24px; font-weight:700">@Model.JobsThisMonth</div>
    <div>Jobs tháng này</div>
  </div>
  <div style="border:1px solid #e5e7eb; border-radius:8px; padding:16px; min-width:160px">
    <div style="font-size:24px; font-weight:700">$@Model.CostThisMonth.ToString("F2")</div>
    <div>API cost tháng</div>
  </div>
  <div style="border:1px solid #e5e7eb; border-radius:8px; padding:16px; min-width:160px">
    <div style="font-size:24px; font-weight:700">@Model.GrossMargin.ToString("P1")</div>
    <div>Gross margin</div>
  </div>
</div>

<h3>Jobs gần đây</h3>
<!-- danh sách 20 jobs gần nhất -->
```

---

### CSS (Minimal)

Dùng **MVP.css** — không cần framework nặng:

```html
<!-- _Layout.cshtml -->
<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>AI Ad Variation Studio</title>
  <link rel="stylesheet" href="https://unpkg.com/mvp.css">
  <style>
    .badge { padding: 2px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .badge.status-done      { background: #22c55e; color: white; }
    .badge.status-error     { background: #ef4444; color: white; }
    .badge.status-pending   { background: #f59e0b; color: white; }
    .badge.status-scripting,
    .badge.status-voicing,
    .badge.status-composing,
    .badge.status-generating { background: #3b82f6; color: white; }
    .progress-steps { display: flex; gap: 6px; margin: 12px 0; flex-wrap: wrap; }
    .step { padding: 4px 10px; background: #e5e7eb; border-radius: 4px; font-size: 12px; }
    .step.done   { background: #bbf7d0; }
    .step.active { background: #93c5fd; font-weight: bold; }
  </style>
</head>
<body>
  <header>
    <nav>
      <a href="/"><strong>AI Ad Variations</strong></a>
      <ul>
        <li><a href="/Jobs">Jobs</a></li>
        <li><a href="/Jobs/New">+ Tạo mới</a></li>
        <li><a href="/Dashboard">Dashboard</a></li>
        <li><a href="/hangfire">Hangfire</a></li>
      </ul>
    </nav>
  </header>
  <main>@RenderBody()</main>
</body>
</html>
```

---

## Giai Đoạn 3: Next.js (Khi Làm SaaS — Sau 20 Paying Customers)

```
frontend/
├── app/
│   ├── (auth)/
│   │   ├── login/page.tsx
│   │   └── register/page.tsx
│   ├── dashboard/
│   │   ├── page.tsx              ← overview + recent jobs
│   │   └── jobs/
│   │       ├── page.tsx          ← job list với search/filter
│   │       ├── new/page.tsx      ← upload wizard: file → describe → generate
│   │       └── [id]/page.tsx     ← progress + 5 video grid + download all
│   └── layout.tsx
│
├── components/
│   ├── JobStatusBadge.tsx
│   ├── VideoVariationCard.tsx    ← video player + hook text + download button
│   ├── HookPreview.tsx           ← hiển thị 5 hooks trước khi render xong
│   └── CostBreakdown.tsx
│
└── lib/
    └── api.ts
```

### [id]/page.tsx — polling với SWR

```typescript
const { data: job } = useSWR(
  `/api/jobs/${id}`,
  fetcher,
  { refreshInterval: job?.status === 'done' ? 0 : 3000 }
)

// Khi status = done: hiển thị grid 5 video
// Khi status != done: hiển thị progress bar 4 bước + spinner
```

---

## Thứ Tự Build Frontend

```
Giai đoạn 1 (Ngày 6):
  ✓ Razor Pages: Index, New, Detail, Dashboard
  ✓ MVP.css layout
  ✓ Auto-refresh khi job đang chạy
  ✓ Download 5 MP4 links

Giai đoạn 2 (Tháng 2, khi có 10+ khách):
  + Thêm: hook preview (hiển thị trước khi render xong)
  + Thêm: "Tạo thêm variation với hook khác" button
  + Thêm: batch upload (nhiều jobs cùng lúc)

Giai đoạn 3 (Tháng 5+, khi có 20 khách):
  + Next.js rebuild từ đầu
  + Auth (Clerk)
  + Billing UI (Stripe)
  + Self-serve UX
```
