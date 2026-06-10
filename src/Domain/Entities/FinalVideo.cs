namespace Domain.Entities;

using Domain.Common;

public class FinalVideo : BaseEntity
{
    public Guid StoryId { get; set; }
    public Story Story { get; set; } = null!;
    public Guid? VideoAssetId { get; set; }
    public Asset? VideoAsset { get; set; }
    public Guid? SubtitleAssetId { get; set; }
    public Asset? SubtitleAsset { get; set; }
    public string Status { get; set; } = "pending";
    public string? ErrorMessage { get; set; }
}
