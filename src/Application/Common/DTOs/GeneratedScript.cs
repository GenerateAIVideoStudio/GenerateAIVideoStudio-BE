namespace Application.Common.DTOs;

using System.Collections.Generic;

public record GeneratedScript(
    string Hook,
    string Body,
    string Cta,
    string FullText,
    int EstimatedDurationSec,
    List<string> SuggestedHooks);
