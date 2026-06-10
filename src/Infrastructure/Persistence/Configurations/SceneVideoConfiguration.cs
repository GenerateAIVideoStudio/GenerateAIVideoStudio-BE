namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SceneVideoConfiguration : IEntityTypeConfiguration<SceneVideo>
{
    public void Configure(EntityTypeBuilder<SceneVideo> builder)
    {
        builder.ToTable("scene_videos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.JobId).HasMaxLength(255);
        builder.HasOne(x => x.Scene).WithOne(x => x.Video).HasForeignKey<SceneVideo>(x => x.SceneId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Storyboard).WithMany().HasForeignKey(x => x.StoryboardId).OnDelete(DeleteBehavior.ClientSetNull);
        builder.HasOne(x => x.VideoAsset).WithMany().HasForeignKey(x => x.VideoAssetId).OnDelete(DeleteBehavior.ClientSetNull);
    }
}
