namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class VideoJobConfiguration : IEntityTypeConfiguration<VideoJob>
{
    public void Configure(EntityTypeBuilder<VideoJob> builder)
    {
        builder.ToTable("video_jobs");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ProductInfo).HasColumnType("jsonb");
        builder.Property(x => x.Script).HasColumnType("jsonb");

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.FlowType);
        builder.HasIndex(x => x.CreatedAt);
    }
}
