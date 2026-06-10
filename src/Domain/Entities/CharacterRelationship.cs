namespace Domain.Entities;

using Domain.Common;

public class CharacterRelationship : BaseEntity
{
    public Guid CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public Guid TargetCharacterId { get; set; }
    public Character TargetCharacter { get; set; } = null!;
    public string RelationshipType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? SinceStoryId { get; set; }
}
