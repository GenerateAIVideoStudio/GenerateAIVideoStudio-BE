namespace Domain.Entities;

using Domain.Common;

public class SceneVideo : BaseEntity
{
    public Guid SceneId { get; set; }
    public Scene Scene { get; set; } = null!;
    public Guid? StoryboardId { get; set; }
    public Storyboard? Storyboard { get; set; }
    public string? PromptUsed { get; set; }
    public Guid? VideoAssetId { get; set; }
    public Asset? VideoAsset { get; set; }
    public string Status { get; set; } = "pending";
    public string? JobId { get; set; }
    public int? DurationSec { get; set; }
    public string? ErrorMessage { get; set; }
}
