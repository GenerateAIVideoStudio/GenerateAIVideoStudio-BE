namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CharacterVoiceConfiguration : IEntityTypeConfiguration<CharacterVoice>
{
    public void Configure(EntityTypeBuilder<CharacterVoice> builder)
    {
        builder.ToTable("character_voices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        builder.Property(x => x.VoiceId).HasMaxLength(255);
        builder.Property(x => x.VoiceName).HasMaxLength(255);
        builder.Property(x => x.Language).HasMaxLength(10).IsRequired();
        builder.HasOne(x => x.Character).WithOne(x => x.Voice).HasForeignKey<CharacterVoice>(x => x.CharacterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.SampleAudioAsset).WithMany().HasForeignKey(x => x.SampleAudioAssetId).OnDelete(DeleteBehavior.SetNull);
    }
}
