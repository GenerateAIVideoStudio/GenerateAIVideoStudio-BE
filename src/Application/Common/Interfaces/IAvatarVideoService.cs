namespace Application.Common.Interfaces;

using Application.Common.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAvatarVideoService
{
    Task<string> SubmitAsync(string audioObjectKey, string avatarId, CancellationToken ct = default);
    Task<AvatarVideoResult?> GetResultAsync(string jobId, CancellationToken ct = default);
    Task<List<AvatarOption>> GetAvailableAvatarsAsync(CancellationToken ct = default);
}
