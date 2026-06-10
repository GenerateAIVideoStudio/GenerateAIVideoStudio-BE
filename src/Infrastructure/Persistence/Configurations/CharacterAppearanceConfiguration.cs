namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CharacterAppearanceConfiguration : IEntityTypeConfiguration<CharacterAppearance>
{
    public void Configure(EntityTypeBuilder<CharacterAppearance> builder)
    {
        builder.ToTable("character_appearances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.HairColor).HasMaxLength(100);
        builder.Property(x => x.HairStyle).HasMaxLength(100);
        builder.Property(x => x.SkinTone).HasMaxLength(100);
        builder.Property(x => x.EyeColor).HasMaxLength(100);
        builder.Property(x => x.Height).HasMaxLength(50);
        builder.Property(x => x.BodyType).HasMaxLength(100);
        builder.HasOne(x => x.Character).WithOne(x => x.Appearance).HasForeignKey<CharacterAppearance>(x => x.CharacterId).OnDelete(DeleteBehavior.Cascade);
    }
}
