namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CharacterPersonalityConfiguration : IEntityTypeConfiguration<CharacterPersonality>
{
    public void Configure(EntityTypeBuilder<CharacterPersonality> builder)
    {
        builder.ToTable("character_personalities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SpeakingStyle).HasMaxLength(100);
        builder.HasOne(x => x.Character).WithOne(x => x.Personality).HasForeignKey<CharacterPersonality>(x => x.CharacterId).OnDelete(DeleteBehavior.Cascade);
    }
}
