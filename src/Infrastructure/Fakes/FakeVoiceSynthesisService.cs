using Application.Common.Interfaces;
using Application.Common.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Fakes;

public class FakeVoiceSynthesisService : IVoiceSynthesisService
{
    public Task<string> SynthesizeAsync(string text, string voiceId, CancellationToken ct = default)
    {
        return Task.FromResult("https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3");
    }

    public Task<List<VoiceOption>> GetAvailableVoicesAsync(string language = "vi", CancellationToken ct = default)
    {
        var voices = new List<VoiceOption>
        {
            new VoiceOption("vi-male-1", "Nam miền Bắc", "male", "vi"),
            new VoiceOption("vi-female-1", "Nữ miền Bắc", "female", "vi"),
            new VoiceOption("vi-female-2", "Nữ miền Nam", "female", "vi")
        };
        return Task.FromResult(voices);
    }
}
