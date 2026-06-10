namespace Domain.Entities;

using Domain.Common;

public class Asset : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string AssetType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public string? Metadata { get; set; } // JSONB
}
