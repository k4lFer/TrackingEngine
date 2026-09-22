using App.Domain.Materials.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Materials;

public class MaterialConfiguration : IEntityTypeConfiguration<TMaterial>
{
    public void Configure(EntityTypeBuilder<TMaterial> builder)
    {
        builder.ToTable("materials", schema: "materials");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();
        builder.Property(m => m.Code).HasColumnName("code").HasMaxLength(30).IsRequired();
        builder.Property(m => m.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(m => m.Unit).HasColumnName("unit").HasMaxLength(20).IsRequired();
        builder.Property(m => m.Active).HasColumnName("active").IsRequired();

        builder.HasIndex(m => m.Code).IsUnique();
    }
}