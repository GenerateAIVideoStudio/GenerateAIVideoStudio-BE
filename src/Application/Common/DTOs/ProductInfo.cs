namespace Application.Common.DTOs;

using System.Collections.Generic;

public record ProductInfo(
    string Name,
    string? Price,
    string? Description,
    List<string> ImageUrls,
    List<string> ReviewHighlights,
    string? Category);
