using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories;

public class ModelImageRepository : IModelImageRepository
{
    private readonly AppDbContext _context;

    public ModelImageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ModelImage>> GetByGenderAsync(string gender, int count, CancellationToken ct = default)
    {
        return await _context.ModelImages
            .Where(m => m.IsActive && m.Gender != null && m.Gender.ToLower() == gender.ToLower())
            .Take(count)
            .ToListAsync(ct);
    }
}
