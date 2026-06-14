namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ModelImageConfiguration : IEntityTypeConfiguration<ModelImage>
{
    public void Configure(EntityTypeBuilder<ModelImage> builder)
    {
        builder.ToTable("model_images");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Gender);
        builder.HasIndex(x => x.IsActive);
    }
}
