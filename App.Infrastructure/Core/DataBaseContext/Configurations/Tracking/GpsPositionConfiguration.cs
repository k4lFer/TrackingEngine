using App.Domain.Tracking.Entities;
using App.Domain.Vehicles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Tracking;

public class GpsPositionConfiguration : IEntityTypeConfiguration<TGpsPosition>
{
    public void Configure(EntityTypeBuilder<TGpsPosition> builder)
    {
        builder.ToTable("gps_positions", schema: "tracking");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();
        builder.Property(p => p.VehicleId).HasColumnName("vehicle_id").HasColumnType("uuid").IsRequired();
        builder.Property(p => p.DeviceId).HasColumnName("device_id").IsRequired();
        builder.Property(p => p.RecordedAt).HasColumnName("recorded_at").IsRequired();
        builder.Property(p => p.ReceivedAt).HasColumnName("received_at").IsRequired();
        builder.Property(p => p.Geometry)
            .HasColumnName("geometry")
            .HasColumnType("geometry (point)")
            .IsRequired();
        builder.Property(p => p.SpeedKmh).HasColumnName("speed_kmh");
        builder.Property(p => p.HeadingDeg).HasColumnName("heading_deg");
        builder.Property(p => p.Ignition).HasColumnName("ignition");
        builder.Property(p => p.OdometerKm).HasColumnName("odometer_km");
        builder.Property(p => p.Hdop).HasColumnName("hdop");
        builder.Property(p => p.Satellites).HasColumnName("satellites");

        builder.HasIndex(p => p.VehicleId);
        builder.HasIndex(p => p.RecordedAt);

        builder.HasOne<TVehicle>()
            .WithMany()
            .HasForeignKey(p => p.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
