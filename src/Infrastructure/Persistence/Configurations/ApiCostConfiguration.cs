namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ApiCostConfiguration : IEntityTypeConfiguration<ApiCost>
{
    public void Configure(EntityTypeBuilder<ApiCost> builder)
    {
        builder.ToTable("api_costs");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.CostUsd).HasPrecision(10, 6);
        builder.Property(x => x.Metadata).HasColumnType("jsonb");

        builder.HasOne(x => x.Job)
               .WithMany(x => x.ApiCosts)
               .HasForeignKey(x => x.JobId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.JobId);
        builder.HasIndex(x => x.Service);
        builder.HasIndex(x => x.CreatedAt);
    }
}
