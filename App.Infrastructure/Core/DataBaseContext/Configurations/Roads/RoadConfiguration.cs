using App.Domain.Routes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Roads;

public class RoadConfiguration : IEntityTypeConfiguration<TMineRoad>
{
    public void Configure(EntityTypeBuilder<TMineRoad> builder)
    {
        builder.ToTable("mine_roads", schema: "roads");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();
        builder.Property(r => r.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(r => r.Geometry)
            .HasColumnName("geometry")
            .HasColumnType("geometry (linestring)")
            .IsRequired();
        builder.Property(r => r.MaxSpeedKmh).HasColumnName("max_speed_kmh");
        builder.Property(r => r.Active).HasColumnName("active").HasColumnType("boolean").IsRequired();

        builder.HasIndex(r => r.Code).IsUnique();
        builder.HasIndex(r => r.Active);
    }
}