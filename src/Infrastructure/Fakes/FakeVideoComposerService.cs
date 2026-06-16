using Application.Common.Interfaces;
using Application.Common.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Fakes;

public class FakeVideoComposerService : IVideoComposerService
{
    public Task<string> ComposeAsync(VideoComposeRequest request, CancellationToken ct = default)
    {
        return Task.FromResult("https://assets.mixkit.co/videos/preview/mixkit-opening-a-delivery-box-with-clothing-42526-large.mp4");
    }
}
