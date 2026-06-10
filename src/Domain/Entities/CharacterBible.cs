namespace Domain.Entities;

using Domain.Common;

public class CharacterBible : BaseEntity
{
    public Guid CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public string BibleJson { get; set; } = "{}";
    public int Version { get; set; } = 1;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
