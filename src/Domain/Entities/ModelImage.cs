namespace Domain.Entities;

using Domain.Common;
using System;

public class ModelImage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string? PreviewUrl { get; set; }
    public string? Gender { get; set; }
    public string? Style { get; set; }
    public string Ethnicity { get; set; } = "vietnamese";
    public bool IsActive { get; set; } = true;
}
