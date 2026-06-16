namespace Application.Common.Interfaces;

using Application.Common.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IKlingService
{
    Task<string> SubmitTryOnAsync(
        string modelImageUrl,
        string garmentImageUrl,
        CancellationToken ct = default);

    Task<string> SubmitImageToVideoAsync(
        string imageUrl,
        string motionPrompt,
        int durationSec = 5,
        CancellationToken ct = default);

    Task<KlingTaskResult?> GetResultAsync(
        string taskId,
        KlingTaskType taskType,
        CancellationToken ct = default);
}
