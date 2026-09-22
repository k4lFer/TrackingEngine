using App.Domain.Geofences.Entities;
using App.Domain.Materials.Entities;
using App.Domain.Routes.Entities;
using App.Domain.Tracking.Entities;
using App.Domain.Vehicles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Tracking;

public class TripConfiguration : IEntityTypeConfiguration<TTrip>
{
    public void Configure(EntityTypeBuilder<TTrip> builder)
    {
        builder.ToTable("trips", schema: "tracking");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();
        builder.Property(t => t.VehicleId).HasColumnName("vehicle_id").HasColumnType("uuid").IsRequired();
        builder.Property(t => t.OriginGeofenceId).HasColumnName("origin_geofence_id");
        builder.Property(t => t.DestinationGeofenceId).HasColumnName("destination_geofence_id");
        builder.Property(t => t.RouteId).HasColumnName("route_id");
        builder.Property(t => t.MaterialId).HasColumnName("material_id");
        builder.Property(t => t.LoadTonnes).HasColumnName("load_tonnes");
        builder.Property(t => t.StartedAt).HasColumnName("started_at").IsRequired();
        builder.Property(t => t.EndedAt).HasColumnName("ended_at");
        builder.Property(t => t.DistanceKm).HasColumnName("distance_km");
        builder.Property(t => t.DurationS).HasColumnName("duration_s");
        builder.Property(t => t.MaxSpeedKmh).HasColumnName("max_speed_kmh");
        builder.Property(t => t.DeviationCount).HasColumnName("deviation_count").IsRequired();
        builder.Property(t => t.SpeedAlertCount).HasColumnName("speed_alert_count").IsRequired();
        builder.Property(t => t.Status).HasColumnName("status").IsRequired();
        builder.Property(t => t.Track)
            .HasColumnName("track")
            .HasColumnType("geometry (linestring)");

        builder.HasIndex(t => t.VehicleId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.RouteId);

        builder.HasOne<TVehicle>()
            .WithMany()
            .HasForeignKey(t => t.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<TRoute>()
            .WithMany()
            .HasForeignKey(t => t.RouteId);
        builder.HasOne<TMaterial>()
            .WithMany()
            .HasForeignKey(t => t.MaterialId);
        builder.HasOne<TGeofence>()
            .WithMany()
            .HasForeignKey(t => t.OriginGeofenceId);
        builder.HasOne<TGeofence>()
            .WithMany()
            .HasForeignKey(t => t.DestinationGeofenceId);
    }
}