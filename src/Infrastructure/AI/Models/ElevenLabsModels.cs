using System.Collections.Generic;

namespace Infrastructure.AI.Models;

internal record ElevenVoicesResponse(List<ElevenVoice> Voices);
internal record ElevenVoice(string VoiceId, string Name);
