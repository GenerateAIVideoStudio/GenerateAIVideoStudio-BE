using Application.Common.DTOs;
using Application.Common.Interfaces;
using Infrastructure.AI.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.AI;

public class KlingService : IKlingService
{
    private readonly HttpClient _http;
    private readonly string _accessKey;
    private readonly string _secretKey;

    public KlingService(HttpClient http, IConfiguration config)
    {
        _http = http;

        _accessKey = config["Kling:AccessKey"]
            ?? throw new ArgumentNullException("Kling:AccessKey configuration is missing");
        _secretKey = config["Kling:SecretKey"]
            ?? throw new ArgumentNullException("Kling:SecretKey configuration is missing");

        _http.BaseAddress = new Uri("https://api.klingai.com");
    }

    private string GenerateKlingToken()
    {
        var payload = new Dictionary<string, object>
        {
            ["iss"] = _accessKey,
            ["exp"] = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds(),
            ["nbf"] = DateTimeOffset.UtcNow.AddSeconds(-5).ToUnixTimeSeconds()
        };

        var header = Base64UrlEncode(JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" }));
        var claims = Base64UrlEncode(JsonSerializer.Serialize(payload));
        var sig = Base64UrlEncode(HMACSHA256($"{header}.{claims}", _secretKey));
        return $"{header}.{claims}.{sig}";
    }

    private string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace("=", "")
            .Replace("+", "-")
            .Replace("/", "_");
    }

    private string Base64UrlEncode(string input)
    {
        return Base64UrlEncode(Encoding.UTF8.GetBytes(input));
    }

    private byte[] HMACSHA256(string message, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
    }

    private void SetAuthorizationHeader()
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateKlingToken());
    }

    public async Task<string> SubmitTryOnAsync(string modelImageUrl, string garmentImageUrl, CancellationToken ct = default)
    {
        SetAuthorizationHeader();

        var body = new
        {
            model_name = "kolors-virtual-try-on-v1-5",
            human_image = modelImageUrl,
            cloth_image = garmentImageUrl
        };

        var response = await _http.PostAsJsonAsync("/v1/images/kolors-virtual-try-on", body, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<KlingTaskRes>(cancellationToken: ct);
        if (result?.Data?.TaskId == null)
        {
            throw new InvalidOperationException("Kling Virtual Try-On API response missing task_id");
        }

        return result.Data.TaskId;
    }

    public async Task<string> SubmitImageToVideoAsync(string imageUrl, string motionPrompt, int durationSec = 5, CancellationToken ct = default)
    {
        SetAuthorizationHeader();

        var body = new
        {
            model_name = "kling-v1-5",
            image = imageUrl,
            prompt = motionPrompt,
            duration = durationSec.ToString(),
            cfg_scale = 0.5,
            mode = "std"
        };

        var response = await _http.PostAsJsonAsync("/v1/videos/image2video", body, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<KlingTaskRes>(cancellationToken: ct);
        if (result?.Data?.TaskId == null)
        {
            throw new InvalidOperationException("Kling Image-to-Video API response missing task_id");
        }

        return result.Data.TaskId;
    }

    public async Task<KlingTaskResult?> GetResultAsync(string taskId, KlingTaskType taskType, CancellationToken ct = default)
    {
        SetAuthorizationHeader();

        var endpoint = taskType == KlingTaskType.TryOn
            ? $"/v1/images/kolors-virtual-try-on/{taskId}"
            : $"/v1/videos/image2video/{taskId}";

        var response = await _http.GetFromJsonAsync<KlingDetailRes>(endpoint, ct);
        if (response?.Data == null)
        {
            return null;
        }

        var status = response.Data.TaskStatus?.ToLower();
        if (status == "processing" || status == "submitted")
        {
            return null; // Still running
        }

        if (status == "succeed")
        {
            var imageUrls = response.Data.TaskResult?.Images?.Select(i => i.Url ?? "").Where(u => !string.IsNullOrEmpty(u)).ToList();
            var videoUrl = response.Data.TaskResult?.Videos?.FirstOrDefault()?.Url;

            return new KlingTaskResult(true, imageUrls, videoUrl, null);
        }

        return new KlingTaskResult(false, null, null, $"Kling task failed with status: {status}");
    }
}
