namespace Domain.Entities;

using Domain.Common;

public class CharacterAppearance : BaseEntity
{
    public Guid CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public string? HairColor { get; set; }
    public string? HairStyle { get; set; }
    public string? SkinTone { get; set; }
    public string? EyeColor { get; set; }
    public string? Height { get; set; }
    public string? BodyType { get; set; }
    public string? TypicalOutfit { get; set; }
    public string? Accessories { get; set; }
    public string? ExtraNotes { get; set; }
}
