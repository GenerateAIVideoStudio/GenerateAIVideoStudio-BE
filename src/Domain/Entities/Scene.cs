namespace Domain.Entities;

using Domain.Common;

public class Scene : BaseEntity
{
    public Guid StoryId { get; set; }
    public Story Story { get; set; } = null!;
    public Guid? LocationId { get; set; }
    public Location? Location { get; set; }
    public int SceneNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Dialogue { get; set; }
    public string? Mood { get; set; }
    public int DurationSec { get; set; } = 7;
    public string Status { get; set; } = "draft";

    public ICollection<SceneCharacter> SceneCharacters { get; set; } = [];
    public Storyboard? Storyboard { get; set; }
    public SceneVideo? Video { get; set; }
    public ICollection<SceneVoice> Voices { get; set; } = [];
}
