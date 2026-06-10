namespace Domain.Entities;

using Domain.Common;

public class CharacterPersonality : BaseEntity
{
    public Guid CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public string[]? Traits { get; set; }
    public string? SpeakingStyle { get; set; }
    public string? Background { get; set; }
    public string? Goals { get; set; }
    public string? Quirks { get; set; }
}
