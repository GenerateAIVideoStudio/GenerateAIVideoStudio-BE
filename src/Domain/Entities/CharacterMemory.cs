namespace Domain.Entities;

using Domain.Common;

public class CharacterMemory : BaseEntity
{
    public Guid CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public Guid? RelatedCharacterId { get; set; }
    public Character? RelatedCharacter { get; set; }
    public string MemoryScope { get; set; } = "global";
    public string? MemoryType { get; set; }
    public Guid? StoryId { get; set; }
    public Guid? SceneId { get; set; }
    public string MemoryJson { get; set; } = "{}";
}
