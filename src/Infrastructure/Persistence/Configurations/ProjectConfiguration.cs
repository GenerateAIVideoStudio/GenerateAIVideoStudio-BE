namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.HasOne(x => x.User).WithMany(x => x.Projects).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.UserId);
    }
}
