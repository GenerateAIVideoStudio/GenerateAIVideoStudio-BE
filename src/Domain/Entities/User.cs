namespace Domain.Entities;

using Domain.Common;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    
    public ICollection<Project> Projects { get; set; } = [];
}
