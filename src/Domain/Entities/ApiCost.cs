namespace Domain.Entities;

using Domain.Common;
using System;
using System.Text.Json;

public class ApiCost : BaseEntity
{
    public Guid JobId { get; set; }
    public string Service { get; set; } = string.Empty;
    public string? Action { get; set; }
    public decimal CostUsd { get; set; }
    public JsonDocument? Metadata { get; set; }

    public VideoJob Job { get; set; } = null!;
}
