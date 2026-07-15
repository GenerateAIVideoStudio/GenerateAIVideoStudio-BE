# Database Schema — Creative Variation Engine
> Cập nhật: 2026-06-18
> Triết lý: 4 bảng cho MVP. Không thêm bảng cho đến khi thực sự cần.
> Bảng `users` chỉ thêm ở giai đoạn SaaS (tháng 5+).

---

## ERD

```
video_jobs
  └─ job_briefs      (1-1: hooks, CTAs, script, voice URL do GPT+ElevenLabs sinh ra)
  └─< job_variations (1-N: 5 variations, mỗi cái có hook riêng + output MP4)
  └─< api_costs      (1-N: track từng API call và cost)
```

---

## Bảng 1: video_jobs

```sql
CREATE TABLE video_jobs (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Thông tin đầu vào từ seller
    product_description TEXT,            -- mô tả sản phẩm + giá (để CTA overlay)
    input_type          VARCHAR(10) NOT NULL DEFAULT 'video',
                                         -- 'video' | 'images'
    input_url           VARCHAR(2000),   -- object key trên R2 (video hoặc ảnh đầu tiên)

    -- Trạng thái pipeline
    status              VARCHAR(30) NOT NULL DEFAULT 'pending',
                                         -- pending | scripting | voicing | composing | generating | done | error
    error_message       TEXT,

    -- Thông tin giao hàng
    customer_email      VARCHAR(255),
    notes               TEXT,

    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at        TIMESTAMPTZ
);

CREATE INDEX idx_video_jobs_status  ON video_jobs(status);
CREATE INDEX idx_video_jobs_created ON video_jobs(created_at DESC);
```

**C# Entity:**
```csharp
public class VideoJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? ProductDescription { get; set; }
    public string InputType { get; set; } = InputType.Video;
    public string? InputUrl { get; set; }       // R2 object key
    public string Status { get; set; } = JobStatus.Pending;
    public string? ErrorMessage { get; set; }
    public string? CustomerEmail { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public JobBrief? Brief { get; set; }
    public List<JobVariation> Variations { get; set; } = [];
    public List<ApiCost> ApiCosts { get; set; } = [];
}
```

---

## Bảng 2: job_briefs

Lưu output của bước Content Intelligence (GPT) và Voice Synthesis (ElevenLabs/FPT AI).
1 job = 1 brief (1-1 relationship).

```sql
CREATE TABLE job_briefs (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    job_id          UUID NOT NULL UNIQUE REFERENCES video_jobs(id) ON DELETE CASCADE,

    -- Output từ GPT (Content Intelligence)
    hooks_json      JSONB,   -- ["Hook 1 text", "Hook 2 text", ..., "Hook 5 text"]
    ctas_json       JSONB,   -- ["CTA 1", "CTA 2", "CTA 3"]
    voice_script    TEXT,    -- 30-45s voiceover script

    -- Output từ Voice Synthesis
    voice_url       VARCHAR(2000),  -- object key trên R2 (.mp3)

    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

**C# Entity:**
```csharp
public class JobBrief
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid JobId { get; set; }
    public JsonDocument? HooksJson { get; set; }  // string[]
    public JsonDocument? CtasJson { get; set; }   // string[]
    public string? VoiceScript { get; set; }
    public string? VoiceUrl { get; set; }          // R2 object key
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public VideoJob Job { get; set; } = null!;
}
```

---

## Bảng 3: job_variations

5 variations mỗi job. Mỗi variation thay 1 hook khác nhau, giữ nguyên CTA và voice.

```sql
CREATE TABLE job_variations (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    job_id           UUID NOT NULL REFERENCES video_jobs(id) ON DELETE CASCADE,
    variation_index  INT NOT NULL,          -- 0, 1, 2, 3, 4

    hook_text        TEXT,                  -- hook được overlay đầu video
    cta_text         TEXT,                  -- CTA text overlay cuối video
    output_url       VARCHAR(2000),         -- object key trên R2 (.mp4 final)

    status           VARCHAR(30) NOT NULL DEFAULT 'pending',
    error_message    TEXT,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_variations_job_id ON job_variations(job_id);
CREATE UNIQUE INDEX idx_variations_job_index ON job_variations(job_id, variation_index);
```

**C# Entity:**
```csharp
public class JobVariation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid JobId { get; set; }
    public int VariationIndex { get; set; }
    public string? HookText { get; set; }
    public string? CtaText { get; set; }
    public string? OutputUrl { get; set; }   // R2 object key
    public string Status { get; set; } = JobStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public VideoJob Job { get; set; } = null!;
}
```

---

## Bảng 4: api_costs

Track mọi API call để tính cost/job và gross margin.

```sql
CREATE TABLE api_costs (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    job_id      UUID NOT NULL REFERENCES video_jobs(id) ON DELETE CASCADE,
    service     VARCHAR(50) NOT NULL,   -- openai | elevenlabs | fptai | r2
    action      VARCHAR(100),           -- content_brief | tts | upload | compose
    cost_usd    DECIMAL(10, 6) NOT NULL DEFAULT 0,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_api_costs_job_id  ON api_costs(job_id);
CREATE INDEX idx_api_costs_service ON api_costs(service);
CREATE INDEX idx_api_costs_created ON api_costs(created_at);
```

---

## Domain Constants (C#)

```csharp
// src/Domain/Constants/InputType.cs
public static class InputType
{
    public const string Video  = "video";   // seller upload raw footage
    public const string Images = "images";  // seller upload 3-5 ảnh sản phẩm
}

// src/Domain/Constants/JobStatus.cs
public static class JobStatus
{
    public const string Pending    = "pending";     // vừa tạo
    public const string Scripting  = "scripting";   // GPT đang sinh hooks/CTA/script
    public const string Voicing    = "voicing";     // ElevenLabs/FPT AI đang TTS
    public const string Composing  = "composing";   // FFmpeg compose base video
    public const string Generating = "generating";  // FFmpeg tạo 5 variations
    public const string Done       = "done";
    public const string Error      = "error";
}

// src/Domain/Constants/AiService.cs
public static class AiService
{
    public const string OpenAi     = "openai";
    public const string ElevenLabs = "elevenlabs";
    public const string FptAi      = "fptai";
    public const string Cloudflare = "r2";
}
```

---

## AppDbContext

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<VideoJob>     VideoJobs     => Set<VideoJob>();
    public DbSet<JobBrief>     JobBriefs     => Set<JobBrief>();
    public DbSet<JobVariation> JobVariations => Set<JobVariation>();
    public DbSet<ApiCost>      ApiCosts      => Set<ApiCost>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

---

## Migration Command

```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Api
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

---

## Giai Đoạn 3 — Thêm SaaS Layer (tháng 5+, chỉ khi 20+ paying customers)

```sql
CREATE TABLE users (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email               VARCHAR(255) NOT NULL UNIQUE,
    name                VARCHAR(255),
    stripe_customer_id  VARCHAR(100),
    plan                VARCHAR(50) NOT NULL DEFAULT 'free',
    jobs_used           INT NOT NULL DEFAULT 0,
    jobs_limit          INT NOT NULL DEFAULT 1,   -- free: 1 job/tháng
    billing_cycle_start DATE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Khi thêm users table thì thêm FK này:
ALTER TABLE video_jobs ADD COLUMN user_id UUID REFERENCES users(id);
```

---

## Mapping Pipeline Step → Status

```
Tạo job                 → status = pending
Bắt đầu GPT call        → status = scripting
GPT xong, bắt đầu TTS   → status = voicing
TTS xong, bắt đầu FFmpeg→ status = composing  (base video)
Base xong, tạo 5 vars   → status = generating (5 variations parallel)
Tất cả xong             → status = done
Bất kỳ bước nào fail    → status = error
```
