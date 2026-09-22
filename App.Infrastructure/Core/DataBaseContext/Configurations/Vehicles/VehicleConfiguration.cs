using App.Domain.Vehicles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Vehicles;

public class VehicleConfiguration : IEntityTypeConfiguration<TVehicle>
{
    public void Configure(EntityTypeBuilder<TVehicle> builder)
    {
        builder.ToTable("vehicles", schema: "fleet");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever()
            .IsRequired();
        builder.Property(v => v.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(v => v.Plate).HasColumnName("plate").HasMaxLength(20).IsRequired();
        builder.Property(v => v.Brand).HasColumnName("brand").HasMaxLength(100);
        builder.Property(v => v.Model).HasColumnName("model").HasMaxLength(100);
        builder.Property(v => v.Active).HasColumnName("active").IsRequired();
        builder.Property(v => v.LastPosition).HasColumnName("last_position").HasColumnType("geometry (point)");
        builder.Property(v => v.LastReportedAt).HasColumnName("last_reported_at");

        builder.HasIndex(v => v.Code).IsUnique();

        builder.HasOne(v => v.CurrentState)
            .WithOne(t => t.Vehicle)
            .HasForeignKey<TVehicleCurrentState>(t => t.VehicleId)
            .IsRequired(false);
    }
}
