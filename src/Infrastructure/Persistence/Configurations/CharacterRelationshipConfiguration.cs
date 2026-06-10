namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CharacterRelationshipConfiguration : IEntityTypeConfiguration<CharacterRelationship>
{
    public void Configure(EntityTypeBuilder<CharacterRelationship> builder)
    {
        builder.ToTable("character_relationships");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RelationshipType).HasMaxLength(100).IsRequired();
        builder.HasOne(x => x.Character).WithMany(x => x.Relationships).HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.TargetCharacter).WithMany().HasForeignKey(x => x.TargetCharacterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.CharacterId);
        builder.HasIndex(x => new { x.CharacterId, x.TargetCharacterId }).IsUnique();
    }
}
