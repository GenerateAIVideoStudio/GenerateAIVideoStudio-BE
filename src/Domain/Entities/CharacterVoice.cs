namespace Domain.Entities;

using Domain.Common;

public class CharacterVoice : BaseEntity
{
    public Guid CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public string Provider { get; set; } = "elevenlabs";
    public string? VoiceId { get; set; }
    public string? VoiceName { get; set; }
    public string Language { get; set; } = "vi";
    public Guid? SampleAudioAssetId { get; set; }
    public Asset? SampleAudioAsset { get; set; }
}
