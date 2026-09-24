using App.Domain.Geofences.Entities;
using App.Domain.Routes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Routes;

public class RouteConfiguration : IEntityTypeConfiguration<TRoute>
{
    public void Configure(EntityTypeBuilder<TRoute> builder)
    {
        builder.ToTable("routes", schema: "routes");

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
        builder.Property(r => r.ToleranceM).HasColumnName("tolerance_m").IsRequired();
        builder.Property(r => r.MaxSpeedKmh).HasColumnName("max_speed_kmh");
        builder.Property(r => r.SpeedProfileJson).HasColumnName("speed_profile_json");
        builder.Property(r => r.WaypointsJson).HasColumnName("waypoints_json");
        builder.Property(r => r.Active).HasColumnName("active").IsRequired();
        builder.Property(r => r.OriginGeofenceId).HasColumnName("origin_geofence_id");
        builder.Property(r => r.DestinationGeofenceId).HasColumnName("destination_geofence_id");
        builder.Property(r => r.RouteGroupId).HasColumnName("route_group_id");
        builder.Property(r => r.AlternativeRank).HasColumnName("alternative_rank").IsRequired().HasDefaultValue(0);

        builder.HasIndex(r => r.Code).IsUnique();
        builder.HasIndex(r => r.Active);
        builder.HasIndex(r => r.RouteGroupId);

        builder.HasOne<TGeofence>()
            .WithMany()
            .HasForeignKey(r => r.OriginGeofenceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TGeofence>()
            .WithMany()
            .HasForeignKey(r => r.DestinationGeofenceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}