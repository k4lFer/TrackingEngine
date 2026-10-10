using System;

namespace App.UseCases.Tracking.Services;

public sealed class PositionProcessInput
{
    public Guid VehicleId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? SpeedKmh { get; set; }
    public double? HeadingDeg { get; set; }
    public int? Satellites { get; set; }
    public double? Hdop { get; set; }
    public bool? Ignition { get; set; }
    public DateTime DeviceTimestamp { get; set; }
}
