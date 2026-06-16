namespace Application.Common.Interfaces;

using Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface IVideoJobRepository
{
    Task<VideoJob?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(VideoJob job, CancellationToken ct = default);
    Task UpdateAsync(VideoJob job, CancellationToken ct = default);
    Task<List<VideoJob>> ListAsync(string? status, string? flowType, int skip, int take, CancellationToken ct = default);
    Task<int> CountAsync(string? status, string? flowType, CancellationToken ct = default);
}
