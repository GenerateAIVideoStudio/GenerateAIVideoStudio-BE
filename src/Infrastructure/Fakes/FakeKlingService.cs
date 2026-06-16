using Application.Common.Interfaces;
using Application.Common.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Fakes;

public class FakeKlingService : IKlingService
{
    public Task<string> SubmitTryOnAsync(
        string modelImageUrl,
        string garmentImageUrl,
        CancellationToken ct = default)
    {
        return Task.FromResult($"kling_tryon_{System.Guid.NewGuid()}");
    }

    public Task<string> SubmitImageToVideoAsync(
        string imageUrl,
        string motionPrompt,
        int durationSec = 5,
        CancellationToken ct = default)
    {
        return Task.FromResult($"kling_i2v_{System.Guid.NewGuid()}");
    }

    public Task<KlingTaskResult?> GetResultAsync(
        string taskId,
        KlingTaskType taskType,
        CancellationToken ct = default)
    {
        KlingTaskResult result;
        if (taskType == KlingTaskType.TryOn)
        {
            result = new KlingTaskResult(
                IsSuccess: true,
                ImageUrls: new List<string> { "https://picsum.photos/400/600?tryon1", "https://picsum.photos/400/600?tryon2" },
                VideoUrl: null,
                ErrorMessage: null
            );
        }
        else
        {
            result = new KlingTaskResult(
                IsSuccess: true,
                ImageUrls: null,
                VideoUrl: "https://assets.mixkit.co/videos/preview/mixkit-hands-holding-a-glass-jar-with-bath-salts-48842-large.mp4",
                ErrorMessage: null
            );
        }
        return Task.FromResult<KlingTaskResult?>(result);
    }
}
