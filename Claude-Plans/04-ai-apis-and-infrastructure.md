# AI APIs & Infrastructure
> Cập nhật: 2026-06-18
> MVP dùng: OpenAI GPT-4o + ElevenLabs/FPT AI + FFmpeg + Cloudflare R2.
> Kling/HeyGen deprioritized đến V6+ (sau khi có 20+ paying customers).

---

## PHẦN 1 — OpenAI: Content Intelligence Service

### Mục đích
Thay thế các service cũ (ScriptGenerator, CategoryDetector, FootageAnalyzer) bằng 1 call duy nhất.
GPT tự chọn **angle phù hợp nhất** cho từng sản phẩm — không hard-code 5 loại hook.

### Implementation

```csharp
public class OpenAiContentIntelligenceService : IContentIntelligenceService
{
    public async Task<ContentBrief> GenerateBriefAsync(
        string productDescription,
        string inputType,
        CancellationToken ct = default)
    {
        var system = """
            Bạn là chuyên gia marketing ads cho TikTok Shop và Shopee Việt Nam.
            Nhiệm vụ: phân tích sản phẩm → chọn 5 ANGLE HAY NHẤT → viết hooks + script.
            
            Quan trọng:
            - KHÔNG hard-code 5 loại hook. Với từng sản phẩm, chọn angles phù hợp nhất.
              VD: máy hút bụi → pain strong (nhà bẩn) + curiosity (cơ chế hoạt động)
                  nước hoa → desire (smell/identity) + social proof
                  áo → transformation (mặc vào khác ngay) + social proof
            - Hooks phải hook trong 3 giây đầu. Gây tò mò, đừng giải thích ngay.
            - Script phải tự nhiên như người thật đang nói — không giống quảng cáo.
            - KHÔNG dùng: "quảng cáo", "sponsored", "tài trợ", "được tài trợ"
            
            Output JSON (không markdown):
            {
              "hooks": ["hook1", "hook2", "hook3", "hook4", "hook5"],
              "ctas": ["CTA1", "CTA2", "CTA3"],
              "voice_script": "30-45s full script..."
            }
            
            Định dạng hook: ngắn (3-8 từ), gây tò mò hoặc đánh trúng nỗi đau/khao khát.
            Định dạng CTA: hành động rõ ràng (comment "MUỐN", bấm link, nhắn tin...).
            Định dạng script: hook (3-5s) → body (15-20s: problem→solution→social proof) → CTA (3-5s).
            """;

        var user = $"""
            Sản phẩm: {productDescription}
            Loại input: {inputType} (video = seller có raw footage; images = chỉ có ảnh)
            
            Hãy:
            1. Phân tích loại sản phẩm và nhu cầu của người mua
            2. Chọn 5 angles hay nhất (cân nhắc: pain, desire, curiosity, transformation, social_proof, comparison, urgency, trend)
            3. Viết 1 hook cho mỗi angle (ngắn, hook trong 3s)
            4. Viết 3 CTAs
            5. Viết full voice script 30-45s
            """;

        var chat = await _client.GetChatClient("gpt-4o")
            .CompleteChatAsync(
                [new SystemChatMessage(system), new UserChatMessage(user)],
                cancellationToken: ct);

        var json = chat.Value.Content[0].Text;
        var result = JsonSerializer.Deserialize<BriefJson>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        return new ContentBrief(result.Hooks, result.Ctas, result.VoiceScript);
    }
}

record BriefJson(
    string[] Hooks,
    string[] Ctas,
    [property: JsonPropertyName("voice_script")] string VoiceScript
);
```

**Pricing**: ~$0.015/call với GPT-4o (1000-1500 tokens in+out)

---

## PHẦN 2 — Voice Synthesis

### Ngày 0: Test trước khi code

Chạy cùng 1 script qua 3 provider → cho 5-10 người nghe → chọn provider tốt nhất.
`IVoiceSynthesisService` đã abstract → chỉ cần đổi implementation trong DI, không phá code.

| Provider | Điểm mạnh | Rủi ro | Setup |
|----------|-----------|--------|-------|
| **FPT AI** | Train riêng tiếng Việt, giọng tự nhiên nhất, có Nam/Nữ Bắc/Nam | Doc ít | https://fpt.ai/tts |
| **Zalo AI** | Miễn phí tier cao | API kém ổn định | https://zalo.ai/tools/ai-voice-generator |
| **ElevenLabs** | SDK tốt, model `eleven_multilingual_v2` | Tiếng Việt chưa chắc tự nhiên | https://elevenlabs.io |

### FPT AI Implementation (ưu tiên test trước)

```csharp
public class FptAiVoiceSynthesisService : IVoiceSynthesisService
{
    // API: https://api.fpt.ai/hmi/tts/v5
    // Header: api-key: {key}
    // Body: text (raw), voice (leminh|myan|giahuy|lannhi|banmai...)
    // Response: stream audio/wav

    public async Task<string> SynthesizeAsync(
        string text, string voiceId, CancellationToken ct = default)
    {
        _http.DefaultRequestHeaders.Add("api-key", _apiKey);

        var response = await _http.PostAsync(
            "https://api.fpt.ai/hmi/tts/v5",
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("text", text),
                new KeyValuePair<string, string>("voice", voiceId),
                new KeyValuePair<string, string>("speed", ""),
                new KeyValuePair<string, string>("id", Guid.NewGuid().ToString())
            }), ct);
        response.EnsureSuccessStatusCode();

        var audioBytes = await response.Content.ReadAsByteArrayAsync(ct);
        var objectKey  = $"audio/{Guid.NewGuid()}.mp3";
        using var stream = new MemoryStream(audioBytes);
        await _storage.UploadAsync(stream, objectKey, "audio/mpeg", ct);
        return objectKey;
    }

    public Task<List<VoiceOption>> GetAvailableVoicesAsync(
        string language = "vi", CancellationToken ct = default) =>
        Task.FromResult(new List<VoiceOption>
        {
            new("leminh",  "Lê Minh",  "Nam Miền Bắc", "vi"),
            new("myan",    "Mỹ An",    "Nữ Miền Nam",  "vi"),
            new("giahuy",  "Gia Huy",  "Nam Miền Nam",  "vi"),
            new("lannhi",  "Lan Nhi",  "Nữ Miền Bắc",  "vi"),
            new("banmai",  "Ban Mai",  "Nữ Miền Bắc",  "vi"),
        });
}
```

### ElevenLabs Implementation (fallback nếu FPT AI kém)

```csharp
public class ElevenLabsVoiceSynthesisService : IVoiceSynthesisService
{
    // Base URL: https://api.elevenlabs.io
    // Header: xi-api-key: {key}
    // Model: eleven_multilingual_v2

    public async Task<string> SynthesizeAsync(
        string text, string voiceId, CancellationToken ct = default)
    {
        var body = new
        {
            text,
            model_id       = "eleven_multilingual_v2",
            voice_settings = new { stability = 0.5, similarity_boost = 0.8 }
        };

        var response = await _http.PostAsJsonAsync(
            $"/v1/text-to-speech/{voiceId}", body, ct);
        response.EnsureSuccessStatusCode();

        var audioBytes = await response.Content.ReadAsByteArrayAsync(ct);
        var objectKey  = $"audio/{Guid.NewGuid()}.mp3";
        using var stream = new MemoryStream(audioBytes);
        await _storage.UploadAsync(stream, objectKey, "audio/mpeg", ct);
        return objectKey;
    }
}
```

**Pricing ElevenLabs**: Free 10k chars/tháng · Starter $5 (30k chars, ~15 jobs) · Creator $22 (100k chars, ~50 jobs)
**Pricing FPT AI**: test API free tier, xem pricing tại fpt.ai

---

## PHẦN 3 — FFmpeg Video Composer

### Cài FFmpeg

```bash
# Windows
winget install ffmpeg

# Linux / Railway (thêm vào nixpacks.toml)
# [phases.setup]
# nixPkgs = ["ffmpeg"]
```

### ComposeBaseAsync — Base Video (Bước 3)

Tạo 1 base video từ raw footage hoặc ảnh + voiceover.
Base video KHÔNG có hook text — hook sẽ được thêm ở bước variation.

**Trường hợp 1: Input là video**
```bash
# Bước 1: Trim 1s đầu và cuối (thường bị run tay)
ffmpeg -y -i raw_footage.mp4 -ss 1 -to {duration-1} -c copy trimmed.mp4

# Bước 2: Full compose — color grade + voiceover + captions + music
ffmpeg -y \
  -i trimmed.mp4 \
  -i voiceover.mp3 \
  -i background_music.mp3 \
  -filter_complex "
    [0:v]scale=1080:1920:force_original_aspect_ratio=increase,
         crop=1080:1920,
         eq=brightness=0.05:saturation=1.3:contrast=1.1,
         unsharp=5:5:0.8:3:3:0.4,
         noise=alls=2:allf=t+u[vcolored];
    [vcolored]subtitles=captions.srt:force_style='
         FontSize=22,Bold=1,
         PrimaryColour=&HFFFFFF,OutlineColour=&H000000,
         Outline=3,Shadow=1,Alignment=2'[vcapt];
    [vcapt]drawtext=text='{product_name_and_price}':
         fontsize=18:fontcolor=white:x=(w-tw)/2:y=h-100:
         box=1:boxcolor=black@0.6:boxborderw=8:
         enable='between(t,{cta_start},{total_duration})'[vout];
    [2:a]volume=0.15,afade=t=out:st={duration-3}:d=3[music];
    [1:a][music]amix=inputs=2:duration=first:weights='1 1'[aout]
  " \
  -map "[vout]" -map "[aout]" \
  -c:v libx264 -preset fast -crf 20 \
  -c:a aac -b:a 192k \
  base_video.mp4
```

**Trường hợp 2: Input là ảnh (slideshow)**
```bash
# Tạo slideshow từ 3-5 ảnh với Ken Burns effect
ffmpeg -y \
  -loop 1 -i img1.jpg \
  -loop 1 -i img2.jpg \
  -loop 1 -i img3.jpg \
  -i voiceover.mp3 \
  -filter_complex "
    [0:v]scale=1080:1920:force_original_aspect_ratio=increase,
         crop=1080:1920,
         zoompan=z='min(zoom+0.001,1.05)':x='iw/2-(iw/zoom/2)':y='ih/2-(ih/zoom/2)':d=150:s=1080x1920[v0];
    [1:v]scale=1080:1920:force_original_aspect_ratio=increase,
         crop=1080:1920[v1];
    [2:v]scale=1080:1920:force_original_aspect_ratio=increase,
         crop=1080:1920[v2];
    [v0][v1]xfade=transition=fade:duration=0.5:offset=4.5[vx1];
    [vx1][v2]xfade=transition=fade:duration=0.5:offset=9[vout_raw];
    [vout_raw]subtitles=captions.srt:force_style='FontSize=18,PrimaryColour=&HFFFFFF,Outline=2'[vout]
  " \
  -map "[vout]" -map 3:a \
  -c:v libx264 -c:a aac -t {voice_duration} \
  base_video.mp4
```

### ComposeVariationAsync — 1 Variation (Bước 4)

Lấy base_video.mp4 và thêm hook text overlay vào 3 giây đầu.
Chạy song song × 5 (Task.WhenAll).

```bash
ffmpeg -y \
  -i base_video.mp4 \
  -filter_complex "
    [0:v]drawtext=
         text='{hook_text}':
         fontsize=36:fontcolor=white:
         x=(w-tw)/2:y=h*0.15:
         box=1:boxcolor=black@0.65:boxborderw=12:
         bordercolor=white@0.3:borderw=2:
         enable='between(t,0,3)'[vout]
  " \
  -map "[vout]" -map 0:a \
  -c:v libx264 -preset fast -crf 20 \
  -c:a copy \
  variation_{index}.mp4
```

**Lưu ý thực tế:**
- Variation render rất nhanh vì chỉ thêm text overlay, không encode lại video
- Dùng `-c:a copy` để không re-encode audio
- Chạy 5 variation song song: tổng thời gian = thời gian của 1 variation

### C# Implementation Skeleton

```csharp
public class FfmpegVideoComposerService : IVideoComposerService
{
    private readonly IStorageService _storage;
    private readonly IConfiguration _config;
    private readonly string _ffmpegPath;

    public async Task<string> ComposeBaseAsync(
        string inputObjectKey,
        string inputType,
        string voiceoverObjectKey,
        string[]? imageObjectKeys = null,
        CancellationToken ct = default)
    {
        // 1. Download files từ R2 vào temp directory
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var inputPath    = await DownloadToTemp(inputObjectKey, tempDir, ct);
            var voicePath    = await DownloadToTemp(voiceoverObjectKey, tempDir, ct);
            var outputPath   = Path.Combine(tempDir, "base_video.mp4");
            var captionsPath = Path.Combine(tempDir, "captions.srt");

            // 2. Generate SRT captions từ voice script (đơn giản: 1 word/0.3s)
            await GenerateSrtAsync(captionsPath, voicePath, ct);

            // 3. Run FFmpeg
            if (inputType == InputType.Video)
                await ComposeFromVideoAsync(inputPath, voicePath, captionsPath, outputPath, ct);
            else
                await ComposeFromImagesAsync(imageObjectKeys ?? [inputPath], voicePath, captionsPath, outputPath, ct);

            // 4. Upload base video lên R2
            var objectKey = $"base_videos/{Guid.NewGuid()}.mp4";
            await using var stream = File.OpenRead(outputPath);
            await _storage.UploadAsync(stream, objectKey, "video/mp4", ct);
            return objectKey;
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    public async Task<string> ComposeVariationAsync(
        string baseVideoObjectKey,
        string hookText,
        string ctaText,
        int variationIndex,
        CancellationToken ct = default)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var basePath   = await DownloadToTemp(baseVideoObjectKey, tempDir, ct);
            var outputPath = Path.Combine(tempDir, $"variation_{variationIndex}.mp4");

            // Escape text cho FFmpeg (dấu nháy đơn, dấu phẩy...)
            var escapedHook = EscapeFFmpegText(hookText);

            var args = $"-y -i \"{basePath}\" " +
                       $"-filter_complex \"[0:v]drawtext=text='{escapedHook}':fontsize=36:" +
                       $"fontcolor=white:x=(w-tw)/2:y=h*0.15:" +
                       $"box=1:boxcolor=black@0.65:boxborderw=12:enable='between(t,0,3)'[vout]\" " +
                       $"-map \"[vout]\" -map 0:a -c:v libx264 -preset fast -crf 20 -c:a copy " +
                       $"\"{outputPath}\"";

            await RunFFmpegAsync(args, ct);

            var objectKey = $"variations/{Guid.NewGuid()}.mp4";
            await using var stream = File.OpenRead(outputPath);
            await _storage.UploadAsync(stream, objectKey, "video/mp4", ct);
            return objectKey;
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private async Task RunFFmpegAsync(string args, CancellationToken ct)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName               = _ffmpegPath,
                Arguments              = args,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            }
        };
        process.Start();
        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"FFmpeg failed: {stderr[^Math.Min(500, stderr.Length)..]}");
    }

    private static string EscapeFFmpegText(string text) =>
        text.Replace("'", "\\'").Replace(":", "\\:").Replace(",", "\\,");
}
```

---

## PHẦN 4 — Cloudflare R2 Storage

### Tại sao R2?
- **Không tính phí egress** — video download tốn băng thông lớn, R2 miễn phí egress
- S3-compatible API → dùng AWSSDK.S3
- $0.015/GB storage, $0.36/triệu requests

```csharp
public class CloudflareR2StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;

    public CloudflareR2StorageService(IConfiguration config)
    {
        _bucket = config["Cloudflare:R2:BucketName"]!;
        _s3 = new AmazonS3Client(
            config["Cloudflare:R2:AccessKeyId"],
            config["Cloudflare:R2:SecretAccessKey"],
            new AmazonS3Config
            {
                ServiceURL    = config["Cloudflare:R2:Endpoint"],
                ForcePathStyle = true
            });
    }

    public async Task<string> UploadAsync(
        Stream stream, string objectKey, string contentType, CancellationToken ct = default)
    {
        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName  = _bucket,
            Key         = objectKey,
            InputStream = stream,
            ContentType = contentType
        }, ct);
        return objectKey;
    }

    public Task<string> GetPresignedUrlAsync(
        string objectKey, int expiryHours = 168, CancellationToken ct = default)
    {
        var url = _s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key        = objectKey,
            Expires    = DateTime.UtcNow.AddHours(expiryHours)
        });
        return Task.FromResult(url);
    }

    public async Task<Stream> DownloadAsync(string objectKey, CancellationToken ct = default)
    {
        var res = await _s3.GetObjectAsync(_bucket, objectKey, ct);
        return res.ResponseStream;
    }

    public Task DeleteAsync(string objectKey, CancellationToken ct = default) =>
        _s3.DeleteObjectAsync(_bucket, objectKey, ct);
}
```

**Setup Cloudflare R2:**
1. Tạo tài khoản Cloudflare → R2 → Create bucket: `ugcads`
2. R2 → Manage API tokens → Create token (Read + Write)
3. Lấy: Endpoint URL, AccessKeyId, SecretAccessKey

---

## appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ugcads;Username=postgres;Password=postgres"
  },
  "OpenAI": {
    "ApiKey": "sk-..."
  },
  "ElevenLabs": {
    "ApiKey": "..."
  },
  "FptAi": {
    "ApiKey": "..."
  },
  "Cloudflare": {
    "R2": {
      "Endpoint":        "https://{accountId}.r2.cloudflarestorage.com",
      "AccessKeyId":     "...",
      "SecretAccessKey": "...",
      "BucketName":      "ugcads"
    }
  },
  "FFmpeg": {
    "Path": "ffmpeg"
  },
  "Hangfire": {
    "WorkerCount": 1
  }
}
```

---

## Railway Deploy

```toml
# railway.toml
[build]
builder = "NIXPACKS"
buildCommand = "dotnet publish src/Api -c Release -o /publish"

[deploy]
startCommand = "dotnet /publish/Api.dll"
restartPolicyType = "ON_FAILURE"
```

```toml
# nixpacks.toml (FFmpeg cần thiết)
[phases.setup]
nixPkgs = ["ffmpeg"]
```

```bash
railway login
railway init
railway add --database postgresql
railway up
# Set env vars trên Railway dashboard (OpenAI, ElevenLabs/FPT AI, Cloudflare R2)
```

**Scale Railway khi cần (không cần build ngay):**
- Hiện tại: API + Hangfire cùng 1 instance, `WorkerCount=1`
- Khi > 10 job/ngày: tách Hangfire Worker ra instance riêng, giữ nguyên code

---

## Bảng Chi Phí Per Job (5 Variations)

| Service | Action | Cost |
|---------|--------|------|
| GPT-4o | hooks + CTAs + script (~1200 tokens) | $0.015 |
| ElevenLabs/FPT AI | TTS 30-45s (~400 chars) | $0.030 |
| FFmpeg | tự host — $0 | $0 |
| Cloudflare R2 | upload 6 files (1 base + 5 var) | $0.002 |
| **TOTAL** | **1 job = 5 variations** | **~$0.047** |

**Khi charge 200k VND/job (~$8):**
- Cost: $0.047 (~1.2k VND)
- Gross profit: 198.8k VND
- **Margin: 99.4%**

---

## DEPRIORITIZED — Kling/HeyGen (Dùng Sau V6)

Các service này đã có code sẵn. Không xóa, không build thêm cho đến khi:
- Có 20+ paying customers và revenue ổn định
- Fashion sellers đặt hàng cụ thể (Try-On)
- Beauty sellers muốn product animation

Khi đó xem lại:
- [Kling Virtual Try-On]: `KlingService.SubmitTryOnAsync`
- [Kling Image-to-Video]: `KlingService.SubmitImageToVideoAsync`
- [HeyGen Avatar]: `HeyGenAvatarVideoService`

**Lý do chưa dùng**: $0.14-1.00/video vs $0.047/job với 5 videos = vendor risk cao, margin thấp hơn nhiều.
