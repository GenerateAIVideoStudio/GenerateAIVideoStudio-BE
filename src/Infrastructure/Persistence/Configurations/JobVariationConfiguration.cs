namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class JobVariationConfiguration : IEntityTypeConfiguration<JobVariation>
{
    public void Configure(EntityTypeBuilder<JobVariation> builder)
    {
        builder.ToTable("job_variations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TryonImages).HasColumnType("jsonb");
        
        builder.HasOne(x => x.Job)
               .WithMany(x => x.Variations)
               .HasForeignKey(x => x.JobId)
               .OnDelete(DeleteBehavior.Cascade);
               
        builder.HasOne(x => x.ModelImage)
               .WithMany()
               .HasForeignKey(x => x.ModelImageId)
               .OnDelete(DeleteBehavior.SetNull);
               
        builder.HasIndex(x => x.JobId);
    }
}
