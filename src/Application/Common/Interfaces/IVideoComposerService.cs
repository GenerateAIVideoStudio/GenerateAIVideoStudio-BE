namespace Application.Common.Interfaces;

using Application.Common.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IVideoComposerService
{
    Task<string> ComposeAsync(VideoComposeRequest request, CancellationToken ct = default);
}
