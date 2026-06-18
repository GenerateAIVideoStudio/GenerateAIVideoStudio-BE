namespace Application.Common.DTOs;

using System.Collections.Generic;

public record VideoComposeRequest(
    string? AvatarVideoObjectKey,    // Flow A
    List<string>? TryOnImageUrls,    // Flow B
    string? ProductVideoObjectKey,   // Flow C
    List<string> ProductImageUrls,   // All flows (fallback/overlay)
    string ScriptFullText,
    string OutputFormat,
    string FlowType,
    string? AudioObjectKey = null
);
