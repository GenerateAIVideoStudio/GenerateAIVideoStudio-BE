namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    public void Configure(EntityTypeBuilder<Story> builder)
    {
        builder.ToTable("stories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.HasOne(x => x.Project).WithMany(x => x.Stories).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.ProjectId);
    }
}
