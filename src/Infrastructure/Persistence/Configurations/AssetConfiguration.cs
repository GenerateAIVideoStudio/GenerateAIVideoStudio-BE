namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("assets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AssetType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ObjectKey).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => x.ObjectKey).IsUnique();
        builder.Property(x => x.MimeType).HasMaxLength(100);
        builder.Property(x => x.Metadata).HasColumnType("nvarchar(max)");
        builder.HasOne(x => x.Project).WithMany(x => x.Assets).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.AssetType);
    }
}
