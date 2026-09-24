using App.Domain.Vehicles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Devices;

public class DeviceConfiguration : IEntityTypeConfiguration<TDevice>
{
    public void Configure(EntityTypeBuilder<TDevice> builder)
    {
        builder.ToTable("devices", schema: "fleet");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();
        builder.Property(d => d.Identifier).HasColumnName("identifier").HasMaxLength(64).IsRequired();
        builder.Property(d => d.Kind).HasColumnName("kind").IsRequired();
        builder.Property(d => d.Model).HasColumnName("model").HasMaxLength(100);
        builder.Property(d => d.VehicleId).HasColumnName("vehicle_id").HasColumnType("uuid");

        builder.HasIndex(d => d.Identifier).IsUnique();
        builder.HasIndex(d => d.VehicleId).IsUnique();
    }
}