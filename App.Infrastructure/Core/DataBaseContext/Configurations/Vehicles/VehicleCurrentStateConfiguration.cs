using App.Domain.Vehicles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Vehicles;

public class VehicleCurrentStateConfiguration : IEntityTypeConfiguration<TVehicleCurrentState>
{
    public void Configure(EntityTypeBuilder<TVehicleCurrentState> builder)
    {
        builder.ToTable("vehicle_current_state", schema: "fleet");

        builder.HasKey(t => t.VehicleId);
        builder.Ignore(t => t.Id);
        builder.Property(t => t.VehicleId).HasColumnName("vehicle_id").HasColumnType("uuid").IsRequired();

        builder.Property(t => t.State).HasColumnName("state").IsRequired();
        builder.Property(t => t.LastReportedAt).HasColumnName("last_recorded_at");
        builder.Property(t => t.LastReceivedAt).HasColumnName("last_received_at");
        builder.Property(t => t.LastGeom).HasColumnName("last_geom").HasColumnType("geometry (point)");
        builder.Property(t => t.CurrentGeofenceId).HasColumnName("current_geofence_id").HasColumnType("uuid");
        builder.Property(t => t.ActiveTripId).HasColumnName("active_trip_id").HasColumnType("uuid");
        builder.Property(t => t.OverSpeedSince).HasColumnName("over_speed_since");
        builder.Property(t => t.OffRouteSince).HasColumnName("off_route_since");
        builder.Property(t => t.OffRouteStreak).HasColumnName("off_route_streak").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");
    }
}
