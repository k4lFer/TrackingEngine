using App.Domain.Geofences.Entities;
using App.Domain.Tracking.Entities;
using App.Domain.Vehicles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Tracking;

public class TrackingEventConfiguration : IEntityTypeConfiguration<TTrackingEvent>
{
    public void Configure(EntityTypeBuilder<TTrackingEvent> builder)
    {
        builder.ToTable("tracking_events", schema: "tracking");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();
        builder.Property(e => e.VehicleId).HasColumnName("vehicle_id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.TripId).HasColumnName("trip_id").HasColumnType("uuid");
        builder.Property(e => e.Type).HasColumnName("type").IsRequired();
        builder.Property(e => e.Severity).HasColumnName("severity").IsRequired();
        builder.Property(e => e.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(e => e.GeofenceId).HasColumnName("geofence_id").HasColumnType("uuid");
        builder.Property(e => e.Position)
            .HasColumnName("position")
            .HasColumnType("geometry (point)");
        builder.Property(e => e.PayloadJson).HasColumnName("payload_json");

        builder.HasIndex(e => e.VehicleId);
        builder.HasIndex(e => e.TripId);
        builder.HasIndex(e => e.OccurredAt);

        builder.HasOne<TVehicle>()
            .WithMany()
            .HasForeignKey(e => e.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Trip)
            .WithMany()
            .HasForeignKey(e => e.TripId);
        builder.HasOne<TGeofence>()
            .WithMany()
            .HasForeignKey(e => e.GeofenceId);
    }
}
