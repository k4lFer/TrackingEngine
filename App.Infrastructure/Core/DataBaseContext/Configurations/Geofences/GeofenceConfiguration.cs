using App.Domain.Geofences.Entities;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Geofences;

public class GeofenceConfiguration : IEntityTypeConfiguration<TGeofence>
{
    public void Configure(EntityTypeBuilder<TGeofence> builder)
    {
        builder.ToTable("geofences", schema: "geofences");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();
        builder.Property(g => g.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(g => g.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(g => g.Kind).HasColumnName("kind").IsRequired();
        builder.Property(g => g.Priority).HasColumnName("priority").IsRequired();
        builder.Property(g => g.MaxSpeedKmh).HasColumnName("max_speed_kmh");
        builder.Property(g => g.Color).HasColumnName("color").HasMaxLength(20);
        builder.Property(g => g.Active).HasColumnName("active").IsRequired();
        builder.Property(g => g.Geometry)
            .HasColumnName("geometry")
            .HasColumnType("geometry (polygon)")
            .IsRequired();
        builder.HasIndex(g => g.Active);

        builder.HasIndex(g => g.Code).IsUnique();
    }
}