namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class HookTemplateConfiguration : IEntityTypeConfiguration<HookTemplate>
{
    public void Configure(EntityTypeBuilder<HookTemplate> builder)
    {
        builder.ToTable("hook_templates");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.Language);
    }
}
