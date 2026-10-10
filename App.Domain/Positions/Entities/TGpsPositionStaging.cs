using System;

namespace App.Domain.Positions.Entities;

public sealed class TGpsPositionStaging
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DeviceId { get; set; } = string.Empty;
    public Guid? VehicleId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? SpeedKmh { get; set; }
    public double? HeadingDeg { get; set; }
    public int? Satellites { get; set; }
    public double? Hdop { get; set; }
    public bool? Ignition { get; set; }
    public DateTime DeviceTimestamp { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public string? Transport { get; set; }
    public string? Decoder { get; set; }
}
