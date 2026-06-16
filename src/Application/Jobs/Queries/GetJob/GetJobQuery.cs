namespace Application.Jobs.Queries.GetJob;

using Domain.Entities;
using MediatR;
using System;

public record GetJobQuery(Guid Id) : IRequest<VideoJob?>;
