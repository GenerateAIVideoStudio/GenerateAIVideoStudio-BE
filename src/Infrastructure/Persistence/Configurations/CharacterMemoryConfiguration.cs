namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CharacterMemoryConfiguration : IEntityTypeConfiguration<CharacterMemory>
{
    public void Configure(EntityTypeBuilder<CharacterMemory> builder)
    {
        builder.ToTable("character_memories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MemoryScope).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MemoryType).HasMaxLength(50);
        builder.Property(x => x.MemoryJson).HasColumnType("jsonb").IsRequired();
        builder.HasOne(x => x.Character).WithMany(x => x.Memories).HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.RelatedCharacter).WithMany().HasForeignKey(x => x.RelatedCharacterId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.CharacterId);
        builder.HasIndex(x => new { x.CharacterId, x.MemoryScope });
        builder.HasIndex(x => new { x.CharacterId, x.RelatedCharacterId });
    }
}
