namespace Domain.Entities;

using Domain.Common;
using System;

public class HookTemplate : BaseEntity
{
    public string? Category { get; set; }
    public string HookText { get; set; } = string.Empty;
    public string Language { get; set; } = "vi";
    public decimal? PerformanceScore { get; set; } = 0.5m;
}
