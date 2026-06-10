namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AiGenerationConfiguration : IEntityTypeConfiguration<AiGeneration>
{
    public void Configure(EntityTypeBuilder<AiGeneration> builder)
    {
        builder.ToTable("ai_generations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(100);
        builder.Property(x => x.CostUsd).HasColumnType("numeric(10,6)");
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.HasOne(x => x.Project).WithMany(x => x.AiGenerations).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.Provider);
    }
}
