namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.StyleJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.HasOne(x => x.Project).WithMany(x => x.Locations).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ImageAsset).WithMany().HasForeignKey(x => x.ImageAssetId).OnDelete(DeleteBehavior.ClientSetNull);
        builder.HasIndex(x => x.ProjectId);
    }
}
