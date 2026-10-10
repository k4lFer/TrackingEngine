using App.Domain.Positions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Positions;

public sealed class GpsPositionStagingConfiguration : IEntityTypeConfiguration<TGpsPositionStaging>
{
    public void Configure(EntityTypeBuilder<TGpsPositionStaging> builder)
    {
        builder.ToTable("gps_positions_staging", "tracking");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DeviceId).HasColumnName("device_id").HasMaxLength(64).IsRequired();
        builder.Property(x => x.VehicleId).HasColumnName("vehicle_id");
        builder.Property(x => x.Latitude).HasColumnName("latitude").HasPrecision(9, 6);
        builder.Property(x => x.Longitude).HasColumnName("longitude").HasPrecision(9, 6);
        builder.Property(x => x.SpeedKmh).HasColumnName("speed_kmh");
        builder.Property(x => x.HeadingDeg).HasColumnName("heading_deg");
        builder.Property(x => x.Satellites).HasColumnName("satellites");
        builder.Property(x => x.Hdop).HasColumnName("hdop");
        builder.Property(x => x.Ignition).HasColumnName("ignition");
        builder.Property(x => x.DeviceTimestamp).HasColumnName("device_timestamp").IsRequired();
        builder.Property(x => x.ReceivedAt).HasColumnName("received_at").IsRequired();
        builder.Property(x => x.Processed).HasColumnName("processed").IsRequired();
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at");
        builder.Property(x => x.Error).HasColumnName("error");
        builder.Property(x => x.Transport).HasColumnName("transport");
        builder.Property(x => x.Decoder).HasColumnName("decoder");

        // Un único índice parcial cubre la query del worker (WHERE !processed
        // ORDER BY received_at) con el mínimo coste de escritura: cada INSERT del
        // hot path toca 1 índice en vez de 4. DeviceId y (VehicleId, DeviceTimestamp)
        // no se consultan jamás sobre staging: solo la tabla final gps_positions.
        builder.HasIndex(x => x.ReceivedAt)
            .HasDatabaseName("ix_gps_positions_staging_received_at_unprocessed")
            .HasFilter("\"processed\" = false");
    }
}
