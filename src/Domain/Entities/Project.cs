namespace Domain.Entities;

using Domain.Common;

public class Project : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "active";
    
    public ICollection<Asset> Assets { get; set; } = [];
    public ICollection<Character> Characters { get; set; } = [];
    public ICollection<Location> Locations { get; set; } = [];
    public ICollection<Story> Stories { get; set; } = [];
    public ICollection<AiGeneration> AiGenerations { get; set; } = [];
}
