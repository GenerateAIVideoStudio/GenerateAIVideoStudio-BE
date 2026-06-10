namespace Domain.Entities;

using System;

public class SceneCharacter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SceneId { get; set; }
    public Scene Scene { get; set; } = null!;
    public Guid CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public string? Role { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
