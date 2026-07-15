using Application.Common.DTOs;
using Application.Common.Interfaces;
using Infrastructure.AI.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.AI;

public class HeyGenAvatarVideoService : IAvatarVideoService
{
    private readonly HttpClient _http;
    private readonly IStorageService _storage;

    public HeyGenAvatarVideoService(HttpClient http, IStorageService storage, IConfiguration config)
    {
        _http = http;
        _storage = storage;

        var apiKey = config["HeyGen:ApiKey"]
            ?? throw new ArgumentNullException("HeyGen:ApiKey configuration is missing");

        _http.BaseAddress = new Uri("https://api.heygen.com");
        _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }

    public async Task<string> SubmitAsync(string audioObjectKey, string avatarId, CancellationToken ct = default)
    {
        // HeyGen needs a publicly accessible audio URL. We generate a 24-hour presigned URL.
        var audioUrl = await _storage.GetPresignedUrlAsync(audioObjectKey, 24, ct);

        var body = new
        {
            video_inputs = new[]
            {
                new
                {
                    character = new { type = "avatar", avatar_id = avatarId, avatar_style = "normal" },
                    voice = new { type = "audio", audio_url = audioUrl },
                    background = new { type = "color", value = "#F5F5F5" }
                }
            },
            dimension = new { width = 1080, height = 1920 }
        };

        var response = await _http.PostAsJsonAsync("/v2/video/generate", body, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<HeyGenSubmitRes>(cancellationToken: ct);
        if (result?.Data?.VideoId == null)
        {
            throw new InvalidOperationException("HeyGen video generation API response missing video_id");
        }

        return result.Data.VideoId;
    }

    public async Task<AvatarVideoResult?> GetResultAsync(string jobId, CancellationToken ct = default)
    {
        var response = await _http.GetFromJsonAsync<HeyGenStatusRes>($"/v1/video_status.get?video_id={jobId}", ct);
        if (response?.Data == null)
        {
            return null;
        }

        var status = response.Data.Status?.ToLower();
        if (status == "completed")
        {
            return new AvatarVideoResult(true, response.Data.VideoUrl, null);
        }
        else if (status == "failed")
        {
            return new AvatarVideoResult(false, null, response.Data.Error?.Message ?? "HeyGen processing failed");
        }

        return null; // Still processing
    }

    public async Task<List<AvatarOption>> GetAvailableAvatarsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<HeyGenAvatarsRes>("/v2/avatars", ct);
            if (response?.Data?.Avatars == null)
            {
                return GetDefaultAvatars();
            }

            return response.Data.Avatars
                .Select(a => new AvatarOption(
                    a.AvatarId ?? "",
                    a.AvatarName ?? "Unnamed Avatar",
                    a.PreviewImageUrl ?? a.PreviewVideoUrl ?? "",
                    a.Gender ?? "unknown"
                ))
                .ToList();
        }
        catch
        {
            // Fallback to defaults if API call fails (e.g. key issue, rate limit)
            return GetDefaultAvatars();
        }
    }

    private List<AvatarOption> GetDefaultAvatars()
    {
        return new List<AvatarOption>
        {
            new("josh_lite_3d_20240101", "Josh", "https://files.heygen.com/avatar/preview/josh_lite_3d_20240101.jpg", "male"),
            new("clara_lite_3d_20240101", "Clara", "https://files.heygen.com/avatar/preview/clara_lite_3d_20240101.jpg", "female"),
            new("avatar_vn_nu_01", "Mai (VN)", "", "female"),
            new("avatar_vn_nam_01", "Nam (VN)", "", "male")
        };
    }
}
