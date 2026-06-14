namespace Domain.Entities;

using Domain.Common;
using Domain.Constants;
using System;
using System.Text.Json;

public class JobVariation : BaseEntity
{
    public Guid JobId { get; set; }
    public int VariationIndex { get; set; }

    // All flows
    public string? HookText { get; set; }
    public string? VoiceId { get; set; }
    public string? AudioUrl { get; set; }

    // Flow A
    public string? AvatarId { get; set; }
    public string? AvatarVideoUrl { get; set; }

    // Flow B
    public Guid? ModelImageId { get; set; }
    public JsonDocument? TryonImages { get; set; }
    public string? KlingTaskId { get; set; }

    // Flow C
    public string? ProductVideoUrl { get; set; }

    // Output
    public string OutputFormat { get; set; } = VideoFormat.Portrait;
    public string? FinalUrl { get; set; }
    public string Status { get; set; } = JobStatus.Pending;
    public string? ErrorMessage { get; set; }

    public VideoJob Job { get; set; } = null!;
    public ModelImage? ModelImage { get; set; }
}
