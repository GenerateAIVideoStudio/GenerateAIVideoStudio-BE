namespace Application.Jobs.Queries.GetJobList;

using Domain.Entities;
using MediatR;
using System.Collections.Generic;

public record JobListResult(List<VideoJob> Items, int TotalCount);

public record GetJobListQuery(
    string? Status,
    string? FlowType,
    int Page = 1,
    int PageSize = 10
) : IRequest<JobListResult>;
