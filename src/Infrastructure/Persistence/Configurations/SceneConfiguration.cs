namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SceneConfiguration : IEntityTypeConfiguration<Scene>
{
    public void Configure(EntityTypeBuilder<Scene> builder)
    {
        builder.ToTable("scenes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(255);
        builder.Property(x => x.Mood).HasMaxLength(100);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.HasOne(x => x.Story).WithMany(x => x.Scenes).HasForeignKey(x => x.StoryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Location).WithMany(x => x.Scenes).HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => new { x.StoryId, x.SceneNumber }).IsUnique();
    }
}
