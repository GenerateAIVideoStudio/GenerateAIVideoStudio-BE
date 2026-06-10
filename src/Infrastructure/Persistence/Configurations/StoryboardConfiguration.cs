namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StoryboardConfiguration : IEntityTypeConfiguration<Storyboard>
{
    public void Configure(EntityTypeBuilder<Storyboard> builder)
    {
        builder.ToTable("storyboards");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.JobId).HasMaxLength(255);
        builder.HasOne(x => x.Scene).WithOne(x => x.Storyboard).HasForeignKey<Storyboard>(x => x.SceneId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ImageAsset).WithMany().HasForeignKey(x => x.ImageAssetId).OnDelete(DeleteBehavior.SetNull);
    }
}
