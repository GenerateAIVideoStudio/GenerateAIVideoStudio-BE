namespace Domain.Entities;

using Domain.Common;

public class Location : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string StyleJson { get; set; } = "{}";
    public Guid? ImageAssetId { get; set; }
    public Asset? ImageAsset { get; set; }
    public ICollection<Scene> Scenes { get; set; } = [];
}
