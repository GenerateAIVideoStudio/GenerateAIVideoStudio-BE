namespace Application.Common.Interfaces;

using Application.Common.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IScriptGeneratorService
{
    Task<GeneratedScript> GenerateAsync(
        ProductInfo product,
        string videoType,
        string targetAudience,
        string? hookText = null,
        CancellationToken ct = default);
}
