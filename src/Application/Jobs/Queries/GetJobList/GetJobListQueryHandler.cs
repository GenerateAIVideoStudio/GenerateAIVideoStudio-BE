namespace Application.Jobs.Queries.GetJobList;

using Application.Common.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

public class GetJobListQueryHandler : IRequestHandler<GetJobListQuery, JobListResult>
{
    private readonly IVideoJobRepository _jobRepo;

    public GetJobListQueryHandler(IVideoJobRepository jobRepo)
    {
        _jobRepo = jobRepo;
    }

    public async Task<JobListResult> Handle(GetJobListQuery request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        var skip = (page - 1) * pageSize;
        var take = pageSize;

        var items = await _jobRepo.ListAsync(request.Status, request.FlowType, skip, take, ct);
        var total = await _jobRepo.CountAsync(request.Status, request.FlowType, ct);

        return new JobListResult(items, total);
    }
}
