namespace Application.Jobs.Queries.GetJob;

using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class GetJobQueryHandler : IRequestHandler<GetJobQuery, VideoJob?>
{
    private readonly IVideoJobRepository _jobRepo;

    public GetJobQueryHandler(IVideoJobRepository jobRepo)
    {
        _jobRepo = jobRepo;
    }

    public async Task<VideoJob?> Handle(GetJobQuery request, CancellationToken ct)
    {
        return await _jobRepo.GetByIdAsync(request.Id, ct);
    }
}
