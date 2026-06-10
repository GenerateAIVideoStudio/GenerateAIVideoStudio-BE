namespace Domain.Entities;

using Domain.Common;

public class Character : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Description { get; set; }
    public Guid? FaceAssetId { get; set; }
    public Asset? FaceAsset { get; set; }
    public string Status { get; set; } = "active";

    public CharacterAppearance? Appearance { get; set; }
    public CharacterPersonality? Personality { get; set; }
    public CharacterVoice? Voice { get; set; }
    public CharacterBible? Bible { get; set; }
    public ICollection<CharacterMemory> Memories { get; set; } = [];
    public ICollection<CharacterRelationship> Relationships { get; set; } = [];
    public ICollection<SceneCharacter> SceneCharacters { get; set; } = [];
}
