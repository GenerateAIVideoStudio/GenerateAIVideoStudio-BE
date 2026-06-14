namespace Infrastructure.Persistence;

using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<VideoJob> VideoJobs => Set<VideoJob>();
    public DbSet<JobVariation> JobVariations => Set<JobVariation>();
    public DbSet<ApiCost> ApiCosts => Set<ApiCost>();
    public DbSet<ModelImage> ModelImages => Set<ModelImage>();
    public DbSet<HookTemplate> HookTemplates => Set<HookTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(ct);
    }
}
