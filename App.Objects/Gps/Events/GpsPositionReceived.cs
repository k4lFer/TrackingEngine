using System;

namespace App.Objects.Gps.Events;

public sealed record GpsPositionReceived
{
    public string? Imei { get; init; }
    public Guid? DeviceId { get; init; }
    public Guid? VehicleId { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double? Speed { get; init; }
    public double? Heading { get; init; }
    public double? Altitude { get; init; }
    public DateTime GpsDateTimeUtc { get; init; }
    public DateTime ReceivedAtUtc { get; init; } = DateTime.UtcNow;
    public string RawFrame { get; init; } = string.Empty;
    public string Protocol { get; init; } = string.Empty;
    public string Source { get; init; } = "tcp";
}
