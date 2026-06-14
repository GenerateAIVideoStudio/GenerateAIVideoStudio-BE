namespace Domain.Entities;

using Domain.Common;
using Domain.Constants;
using System;
using System.Collections.Generic;
using System.Text.Json;

public class VideoJob : BaseEntity
{
    public string? ProductUrl { get; set; }
    public JsonDocument? ProductInfo { get; set; }
    public string? VideoType { get; set; }
    public string? TargetAudience { get; set; }
    public string? CustomerEmail { get; set; }
    public string? Notes { get; set; }
    public string FlowType { get; set; } = Domain.Constants.FlowType.Avatar;
    public JsonDocument? Script { get; set; }
    public string Status { get; set; } = JobStatus.Pending;
    public string? ErrorMessage { get; set; }
    public string? OutputUrl { get; set; }
    public string OutputFormat { get; set; } = VideoFormat.Portrait;
    public DateTime? CompletedAt { get; set; }

    public List<JobVariation> Variations { get; set; } = [];
    public List<ApiCost> ApiCosts { get; set; } = [];
}
