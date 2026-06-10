namespace Domain.Entities;

using System;

public class AiGeneration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? Prompt { get; set; }
    public string? Response { get; set; }
    public int? TokenInput { get; set; }
    public int? TokenOutput { get; set; }
    public decimal? CostUsd { get; set; }
    public string Status { get; set; } = "pending";
    public string? ErrorMessage { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
