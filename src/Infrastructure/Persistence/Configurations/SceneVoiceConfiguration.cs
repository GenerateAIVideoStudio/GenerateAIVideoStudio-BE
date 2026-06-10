namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SceneVoiceConfiguration : IEntityTypeConfiguration<SceneVoice>
{
    public void Configure(EntityTypeBuilder<SceneVoice> builder)
    {
        builder.ToTable("scene_voices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.JobId).HasMaxLength(255);
        builder.Property(x => x.DurationSec).HasColumnType("numeric(6,2)");
        builder.HasOne(x => x.Scene).WithMany(x => x.Voices).HasForeignKey(x => x.SceneId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Character).WithMany().HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.AudioAsset).WithMany().HasForeignKey(x => x.AudioAssetId).OnDelete(DeleteBehavior.SetNull);
    }
}
