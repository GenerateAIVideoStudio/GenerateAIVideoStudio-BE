namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.ToTable("characters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Gender).HasMaxLength(50);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.HasOne(x => x.Project).WithMany(x => x.Characters).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.FaceAsset).WithMany().HasForeignKey(x => x.FaceAssetId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.ProjectId);
    }
}
