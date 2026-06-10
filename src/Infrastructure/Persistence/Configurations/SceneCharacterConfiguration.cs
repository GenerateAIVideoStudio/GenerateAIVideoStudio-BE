namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SceneCharacterConfiguration : IEntityTypeConfiguration<SceneCharacter>
{
    public void Configure(EntityTypeBuilder<SceneCharacter> builder)
    {
        builder.ToTable("scene_characters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Role).HasMaxLength(100);
        builder.HasOne(x => x.Scene).WithMany(x => x.SceneCharacters).HasForeignKey(x => x.SceneId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Character).WithMany(x => x.SceneCharacters).HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.ClientCascade);
        builder.HasIndex(x => x.SceneId);
        builder.HasIndex(x => x.CharacterId);
        builder.HasIndex(x => new { x.SceneId, x.CharacterId }).IsUnique();
    }
}
