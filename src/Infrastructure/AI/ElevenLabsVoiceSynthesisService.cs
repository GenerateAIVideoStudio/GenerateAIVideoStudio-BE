using Application.Common.DTOs;
using Application.Common.Interfaces;
using Infrastructure.AI.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.AI;

public class ElevenLabsVoiceSynthesisService : IVoiceSynthesisService
{
    private readonly HttpClient _http;
    private readonly IStorageService _storage;

    public ElevenLabsVoiceSynthesisService(HttpClient http, IStorageService storage, IConfiguration config)
    {
        _http = http;
        _storage = storage;

        var apiKey = config["ElevenLabs:ApiKey"]
            ?? throw new ArgumentNullException("ElevenLabs:ApiKey configuration is missing");

        _http.BaseAddress = new Uri("https://api.elevenlabs.io");
        _http.DefaultRequestHeaders.Add("xi-api-key", apiKey);
    }

    public async Task<string> SynthesizeAsync(string text, string voiceId, CancellationToken ct = default)
    {
        var body = new
        {
            text,
            model_id = "eleven_multilingual_v2",
            voice_settings = new { stability = 0.5, similarity_boost = 0.8 }
        };

        var response = await _http.PostAsJsonAsync($"/v1/text-to-speech/{voiceId}", body, ct);
        response.EnsureSuccessStatusCode();

        var audioBytes = await response.Content.ReadAsByteArrayAsync(ct);
        var objectKey = $"audio/{Guid.NewGuid()}.mp3";
        using var stream = new MemoryStream(audioBytes);
        await _storage.UploadAsync(stream, objectKey, "audio/mpeg", ct);
        return objectKey;
    }

    public async Task<List<VoiceOption>> GetAvailableVoicesAsync(string language = "vi", CancellationToken ct = default)
    {
        var response = await _http.GetFromJsonAsync<ElevenVoicesResponse>("/v1/voices", ct);
        if (response?.Voices == null)
        {
            return new List<VoiceOption>();
        }

        return response.Voices
            .Select(v => new VoiceOption(v.VoiceId, v.Name, "unknown", language))
            .ToList();
    }
}
