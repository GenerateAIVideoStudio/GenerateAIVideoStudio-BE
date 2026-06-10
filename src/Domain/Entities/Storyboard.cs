namespace Domain.Entities;

using Domain.Common;

public class Storyboard : BaseEntity
{
    public Guid SceneId { get; set; }
    public Scene Scene { get; set; } = null!;
    public string? PromptUsed { get; set; }
    public Guid? ImageAssetId { get; set; }
    public Asset? ImageAsset { get; set; }
    public string Status { get; set; } = "pending";
    public string? JobId { get; set; }
    public string? ErrorMessage { get; set; }
}
