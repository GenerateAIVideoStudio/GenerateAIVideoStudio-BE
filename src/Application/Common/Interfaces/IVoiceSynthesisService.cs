namespace Application.Common.Interfaces;

using Application.Common.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IVoiceSynthesisService
{
    Task<string> SynthesizeAsync(string text, string voiceId, CancellationToken ct = default);
    Task<List<VoiceOption>> GetAvailableVoicesAsync(string language = "vi", CancellationToken ct = default);
}
