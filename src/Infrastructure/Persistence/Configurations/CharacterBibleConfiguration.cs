namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CharacterBibleConfiguration : IEntityTypeConfiguration<CharacterBible>
{
    public void Configure(EntityTypeBuilder<CharacterBible> builder)
    {
        builder.ToTable("character_bibles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BibleJson).HasColumnType("jsonb").IsRequired();
        builder.HasOne(x => x.Character).WithOne(x => x.Bible).HasForeignKey<CharacterBible>(x => x.CharacterId).OnDelete(DeleteBehavior.Cascade);
    }
}
