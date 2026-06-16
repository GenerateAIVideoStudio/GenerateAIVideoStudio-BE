namespace Application.Common.DTOs;

using System.Collections.Generic;

public record KlingTaskResult(
    bool IsSuccess,
    List<string>? ImageUrls,
    string? VideoUrl,
    string? ErrorMessage);
