namespace Domain.Entities;

using Domain.Common;

public class SceneVoice : BaseEntity
{
    public Guid SceneId { get; set; }
    public Scene Scene { get; set; } = null!;
    public Guid? CharacterId { get; set; }
    public Character? Character { get; set; }
    public string DialogueText { get; set; } = string.Empty;
    public Guid? AudioAssetId { get; set; }
    public Asset? AudioAsset { get; set; }
    public string Status { get; set; } = "pending";
    public string? JobId { get; set; }
    public decimal? DurationSec { get; set; }
    public string? ErrorMessage { get; set; }
}
