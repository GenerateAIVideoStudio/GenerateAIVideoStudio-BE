namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.AvatarUrl).HasMaxLength(500);
    }
}
