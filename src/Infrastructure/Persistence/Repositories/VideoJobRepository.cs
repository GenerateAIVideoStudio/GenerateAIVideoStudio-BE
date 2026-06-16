using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories;

public class VideoJobRepository : IVideoJobRepository
{
    private readonly AppDbContext _context;

    public VideoJobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VideoJob?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.VideoJobs
            .Include(j => j.Variations)
            .Include(j => j.ApiCosts)
            .FirstOrDefaultAsync(j => j.Id == id, ct);
    }

    public async Task AddAsync(VideoJob job, CancellationToken ct = default)
    {
        await _context.VideoJobs.AddAsync(job, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(VideoJob job, CancellationToken ct = default)
    {
        var entry = _context.Entry(job);
        if (entry.State == EntityState.Detached)
        {
            _context.VideoJobs.Update(job);
        }

        foreach (var e in _context.ChangeTracker.Entries())
        {
            if (e.Entity is Domain.Common.BaseEntity)
            {
                var idProp = e.Property("Id");
                if (e.State == EntityState.Modified && idProp.OriginalValue is Guid originalGuid && originalGuid == Guid.Empty)
                {
                    e.State = EntityState.Added;
                }
            }
        }

        foreach (var e in _context.ChangeTracker.Entries())
        {
            var keyVal = e.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue;
            Console.WriteLine($"[DIAGNOSTIC] Entity: {e.Entity.GetType().Name}, State: {e.State}, Key: {keyVal}");
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<VideoJob>> ListAsync(string? status, string? flowType, int skip, int take, CancellationToken ct = default)
    {
        var query = _context.VideoJobs.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(j => j.Status == status);
        }

        if (!string.IsNullOrEmpty(flowType))
        {
            query = query.Where(j => j.FlowType == flowType);
        }

        return await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(string? status, string? flowType, CancellationToken ct = default)
    {
        var query = _context.VideoJobs.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(j => j.Status == status);
        }

        if (!string.IsNullOrEmpty(flowType))
        {
            query = query.Where(j => j.FlowType == flowType);
        }

        return await query.CountAsync(ct);
    }
}
