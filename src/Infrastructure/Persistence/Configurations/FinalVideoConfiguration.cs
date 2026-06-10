namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FinalVideoConfiguration : IEntityTypeConfiguration<FinalVideo>
{
    public void Configure(EntityTypeBuilder<FinalVideo> builder)
    {
        builder.ToTable("final_videos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.HasOne(x => x.Story).WithMany(x => x.FinalVideos).HasForeignKey(x => x.StoryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.VideoAsset).WithMany().HasForeignKey(x => x.VideoAssetId).OnDelete(DeleteBehavior.ClientSetNull);
        builder.HasOne(x => x.SubtitleAsset).WithMany().HasForeignKey(x => x.SubtitleAssetId).OnDelete(DeleteBehavior.ClientSetNull);
    }
}
