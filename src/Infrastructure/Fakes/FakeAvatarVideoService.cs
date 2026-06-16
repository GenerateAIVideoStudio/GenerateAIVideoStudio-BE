using Application.Common.Interfaces;
using Application.Common.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Fakes;

public class FakeAvatarVideoService : IAvatarVideoService
{
    public Task<string> SubmitAsync(string audioObjectKey, string avatarId, CancellationToken ct = default)
    {
        return Task.FromResult($"heygen_job_{System.Guid.NewGuid()}");
    }

    public Task<AvatarVideoResult?> GetResultAsync(string jobId, CancellationToken ct = default)
    {
        var result = new AvatarVideoResult(
            IsSuccess: true,
            VideoUrl: "https://assets.mixkit.co/videos/preview/mixkit-girl-in-neon-light-talking-to-camera-42207-large.mp4",
            ErrorMessage: null
        );
        return Task.FromResult<AvatarVideoResult?>(result);
    }

    public Task<List<AvatarOption>> GetAvailableAvatarsAsync(CancellationToken ct = default)
    {
        var avatars = new List<AvatarOption>
        {
            new AvatarOption("avatar-male-1", "John Smart", "https://picsum.photos/200/200?avatar1", "male"),
            new AvatarOption("avatar-female-1", "Anna Professional", "https://picsum.photos/200/200?avatar2", "female")
        };
        return Task.FromResult(avatars);
    }
}
