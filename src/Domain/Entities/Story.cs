namespace Domain.Entities;

using Domain.Common;

public class Story : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    
    public ICollection<Scene> Scenes { get; set; } = [];
    public ICollection<FinalVideo> FinalVideos { get; set; } = [];
}
