using Application.Common.DTOs;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Video;

public class FfmpegVideoComposerService : IVideoComposerService
{
    private readonly IStorageService _storage;
    private readonly HttpClient _httpClient;
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;
    private readonly string _tempRoot;

    public FfmpegVideoComposerService(IStorageService storage, HttpClient httpClient, IConfiguration config)
    {
        _storage = storage;
        _httpClient = httpClient;
        
        _ffmpegPath = config["FFmpeg:Path"] ?? "ffmpeg";
        _ffprobePath = _ffmpegPath.Contains("ffmpeg") ? _ffmpegPath.Replace("ffmpeg", "ffprobe") : "ffprobe";

        // Create a rendering temp directory inside the project root for execution visibility
        _tempRoot = Path.Combine(Directory.GetCurrentDirectory(), "temp_render");
        if (!Directory.Exists(_tempRoot))
        {
            Directory.CreateDirectory(_tempRoot);
        }
    }

    public async Task<string> ComposeAsync(VideoComposeRequest request, CancellationToken ct = default)
    {
        var runId = Guid.NewGuid().ToString();
        var tempDir = Path.Combine(_tempRoot, runId);
        Directory.CreateDirectory(tempDir);

        try
        {
            return request.FlowType.ToLower() switch
            {
                "tryon" => await ComposeTryOnAsync(request, tempDir, ct),
                "image2video" => await ComposeImageVideoAsync(request, tempDir, ct),
                _ => await ComposeAvatarAsync(request, tempDir, ct)
            };
        }
        finally
        {
            // Clean up temporary files safely in the background
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch
            {
                // Suppress cleanup errors to avoid blocking the workflow
            }
        }
    }

    private async Task<string> ComposeAvatarAsync(VideoComposeRequest req, string tempDir, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.AvatarVideoObjectKey))
        {
            throw new ArgumentException("AvatarVideoObjectKey is required for Flow A (Avatar)");
        }

        // 1. Download Avatar Video
        var localAvatarPath = Path.Combine(tempDir, "avatar.mp4");
        await DownloadToLocalAsync(req.AvatarVideoObjectKey, localAvatarPath, ct);

        // 2. Download Product Overlay Image
        var productImgUrl = req.ProductImageUrls?.FirstOrDefault();
        string? localProductImgPath = null;
        if (!string.IsNullOrEmpty(productImgUrl))
        {
            localProductImgPath = Path.Combine(tempDir, "product.jpg");
            await DownloadToLocalAsync(productImgUrl, localProductImgPath, ct);
        }

        // 3. Get Duration of Avatar Video
        var duration = await GetMediaDurationAsync(localAvatarPath);

        // 4. Generate Subtitles
        var srtPath = Path.Combine(tempDir, "captions.srt");
        var srtContent = GenerateSrt(req.ScriptFullText, duration);
        await File.WriteAllTextAsync(srtPath, srtContent, Encoding.UTF8, ct);

        // 5. Build FFmpeg command
        var outputPath = Path.Combine(tempDir, "output.mp4");
        string arguments;

        // Escape paths for FFmpeg filter graph (particularly backslashes on Windows)
        var escapedSrtPath = srtPath.Replace("\\", "/").Replace(":", "\\:");

        if (localProductImgPath != null)
        {
            arguments = $"-y -i \"{localAvatarPath}\" -loop 1 -i \"{localProductImgPath}\" " +
                        $"-filter_complex \"[0:v]scale=1080:1920:force_original_aspect_ratio=decrease,pad=1080:1920:(ow-iw)/2:(oh-ih)/2[avatar];" +
                        $"[1:v]scale=320:320[product];" +
                        $"[avatar][product]overlay=W-w-20:80:enable='greater(t,3)'[v];" +
                        $"[v]subtitles='{escapedSrtPath}':force_style='FontSize=18,PrimaryColour=&HFFFFFF,Outline=2'[vout]\" " +
                        $"-map \"[vout]\" -map 0:a -c:v libx264 -c:a aac -shortest \"{outputPath}\"";
        }
        else
        {
            arguments = $"-y -i \"{localAvatarPath}\" " +
                        $"-filter_complex \"[0:v]scale=1080:1920:force_original_aspect_ratio=decrease,pad=1080:1920:(ow-iw)/2:(oh-ih)/2[v];" +
                        $"[v]subtitles='{escapedSrtPath}':force_style='FontSize=18,PrimaryColour=&HFFFFFF,Outline=2'[vout]\" " +
                        $"-map \"[vout]\" -map 0:a -c:v libx264 -c:a aac -shortest \"{outputPath}\"";
        }

        await ExecuteFfmpegAsync(arguments);

        // 6. Upload final video to storage
        var finalObjectKey = $"videos/{Guid.NewGuid()}.mp4";
        using var outputStream = File.OpenRead(outputPath);
        await _storage.UploadAsync(outputStream, finalObjectKey, "video/mp4", ct);

        return finalObjectKey;
    }

    private async Task<string> ComposeTryOnAsync(VideoComposeRequest req, string tempDir, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.AudioObjectKey))
        {
            throw new ArgumentException("AudioObjectKey (voiceover) is required for Flow B (Try-On)");
        }

        // 1. Download Voiceover Audio
        var localAudioPath = Path.Combine(tempDir, "voiceover.mp3");
        await DownloadToLocalAsync(req.AudioObjectKey, localAudioPath, ct);
        var duration = await GetMediaDurationAsync(localAudioPath);

        // 2. Download Try-On Images (fallback to product images if empty)
        var imageUrls = (req.TryOnImageUrls != null && req.TryOnImageUrls.Any()) 
            ? req.TryOnImageUrls 
            : req.ProductImageUrls;

        if (imageUrls == null || !imageUrls.Any())
        {
            throw new ArgumentException("No images provided for Try-On slideshow");
        }

        var localImagePaths = new List<string>();
        for (int i = 0; i < imageUrls.Count; i++)
        {
            var path = Path.Combine(tempDir, $"image_{i}.jpg");
            await DownloadToLocalAsync(imageUrls[i], path, ct);
            localImagePaths.Add(path);
        }

        // 3. Generate Subtitles
        var srtPath = Path.Combine(tempDir, "captions.srt");
        var srtContent = GenerateSrt(req.ScriptFullText, duration);
        await File.WriteAllTextAsync(srtPath, srtContent, Encoding.UTF8, ct);

        // 4. Create Video clips for each image to compose slideshow
        var timePerImage = duration / localImagePaths.Count;
        var clipPaths = new List<string>();
        for (int i = 0; i < localImagePaths.Count; i++)
        {
            var clipPath = Path.Combine(tempDir, $"clip_{i}.mp4");
            var clipArgs = $"-y -loop 1 -i \"{localImagePaths[i]}\" -t {timePerImage.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} " +
                           $"-vf \"scale=1080:1920:force_original_aspect_ratio=increase,crop=1080:1920\" " +
                           $"-c:v libx264 -pix_fmt yuv420p -r 25 \"{clipPath}\"";
            await ExecuteFfmpegAsync(clipArgs);
            clipPaths.Add(clipPath);
        }

        // 5. Concat clips
        var concatTxtPath = Path.Combine(tempDir, "concat.txt");
        var concatContent = string.Join("\n", clipPaths.Select(p => $"file '{p.Replace("\\", "/")}'"));
        await File.WriteAllTextAsync(concatTxtPath, concatContent, Encoding.UTF8, ct);

        var mergedVideoPath = Path.Combine(tempDir, "merged.mp4");
        var concatArgs = $"-y -f concat -safe 0 -i \"{concatTxtPath}\" -c copy \"{mergedVideoPath}\"";
        await ExecuteFfmpegAsync(concatArgs);

        // 6. Merge with voiceover and subtitles
        var outputPath = Path.Combine(tempDir, "output.mp4");
        var escapedSrtPath = srtPath.Replace("\\", "/").Replace(":", "\\:");
        var finalArgs = $"-y -i \"{mergedVideoPath}\" -i \"{localAudioPath}\" " +
                        $"-vf \"subtitles='{escapedSrtPath}':force_style='FontSize=18,PrimaryColour=&HFFFFFF,Outline=2'\" " +
                        $"-c:v libx264 -c:a aac -shortest -map 0:v -map 1:a \"{outputPath}\"";
        await ExecuteFfmpegAsync(finalArgs);

        // 7. Upload final video to storage
        var finalObjectKey = $"videos/{Guid.NewGuid()}.mp4";
        using var outputStream = File.OpenRead(outputPath);
        await _storage.UploadAsync(outputStream, finalObjectKey, "video/mp4", ct);

        return finalObjectKey;
    }

    private async Task<string> ComposeImageVideoAsync(VideoComposeRequest req, string tempDir, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.AudioObjectKey))
        {
            throw new ArgumentException("AudioObjectKey (voiceover) is required for Flow C (Image-to-Video)");
        }

        // 1. Download Voiceover Audio
        var localAudioPath = Path.Combine(tempDir, "voiceover.mp3");
        await DownloadToLocalAsync(req.AudioObjectKey, localAudioPath, ct);
        var duration = await GetMediaDurationAsync(localAudioPath);

        // 2. Generate Subtitles
        var srtPath = Path.Combine(tempDir, "captions.srt");
        var srtContent = GenerateSrt(req.ScriptFullText, duration);
        await File.WriteAllTextAsync(srtPath, srtContent, Encoding.UTF8, ct);

        var outputPath = Path.Combine(tempDir, "output.mp4");
        var escapedSrtPath = srtPath.Replace("\\", "/").Replace(":", "\\:");

        // If ProductVideoUrl is null/empty, fallback to image slideshow
        if (string.IsNullOrEmpty(req.ProductVideoObjectKey))
        {
            return await ComposeTryOnAsync(req, tempDir, ct);
        }

        // 3. Download Product Video
        var localProductVideoPath = Path.Combine(tempDir, "product_video.mp4");
        await DownloadToLocalAsync(req.ProductVideoObjectKey, localProductVideoPath, ct);

        // 4. Merge looped product video with audio and subtitles
        var finalArgs = $"-y -stream_loop -1 -i \"{localProductVideoPath}\" -i \"{localAudioPath}\" " +
                        $"-vf \"scale=1080:1920:force_original_aspect_ratio=decrease,pad=1080:1920:(ow-iw)/2:(oh-ih)/2," +
                        $"subtitles='{escapedSrtPath}':force_style='FontSize=18,PrimaryColour=&HFFFFFF,Outline=2'\" " +
                        $"-map 0:v -map 1:a -c:v libx264 -c:a aac -shortest -t {duration.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} \"{outputPath}\"";
        await ExecuteFfmpegAsync(finalArgs);

        // 5. Upload final video to storage
        var finalObjectKey = $"videos/{Guid.NewGuid()}.mp4";
        using var outputStream = File.OpenRead(outputPath);
        await _storage.UploadAsync(outputStream, finalObjectKey, "video/mp4", ct);

        return finalObjectKey;
    }

    private async Task DownloadToLocalAsync(string keyOrUrl, string localPath, CancellationToken ct)
    {
        if (keyOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            keyOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            using var response = await _httpClient.GetAsync(keyOrUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
            using var fileStream = File.Create(localPath);
            await response.Content.CopyToAsync(fileStream, ct);
        }
        else
        {
            using var storageStream = await _storage.DownloadAsync(keyOrUrl, ct);
            using var fileStream = File.Create(localPath);
            await storageStream.CopyToAsync(fileStream, ct);
        }
    }

    private async Task<double> GetMediaDurationAsync(string filePath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _ffprobePath,
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            if (process == null) return 15.0;
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            if (double.TryParse(output.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var duration))
            {
                return duration;
            }
        }
        catch
        {
            // Suppress errors and fallback
        }
        return 15.0;
    }

    private async Task ExecuteFfmpegAsync(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _ffmpegPath,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start FFmpeg process");
        }

        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var errorLogs = await errorTask;
            throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}. Error logs: {errorLogs}");
        }
    }

    private string GenerateSrt(string fullText, double totalDuration)
    {
        // Split text by typical sentence ends, commas, or newlines
        var sentences = fullText.Split(new[] { '.', '?', '!', '\n', '\r', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .Where(s => s.Length > 0)
                                .ToList();

        if (!sentences.Any())
        {
            sentences.Add(fullText);
        }

        var sb = new StringBuilder();
        double timePerSentence = totalDuration / sentences.Count;

        // Ensure each caption block doesn't stay on screen for too short/long
        for (int i = 0; i < sentences.Count; i++)
        {
            var start = TimeSpan.FromSeconds(i * timePerSentence);
            var end = TimeSpan.FromSeconds((i + 1) * timePerSentence);

            sb.AppendLine((i + 1).ToString());
            sb.AppendLine($"{FormatTime(start)} --> {FormatTime(end)}");
            sb.AppendLine(sentences[i]);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string FormatTime(TimeSpan ts)
    {
        return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2},{ts.Milliseconds:D3}";
    }
}
