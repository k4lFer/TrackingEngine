using App.Shared.Domain;
using NetTopologySuite.Geometries;

namespace App.Domain.Tracking.Entities;

public class TGpsPosition : BaseDomain
{
    public Guid VehicleId { get; private set; }
    public int DeviceId { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public Point Geometry { get; private set; } = null!;
    public decimal? SpeedKmh { get; private set; }
    public short? HeadingDeg { get; private set; }
    public bool? Ignition { get; private set; }
    public decimal? OdometerKm { get; private set; }
    public decimal? Hdop { get; private set; }
    public short? Satellites { get; private set; }

    private TGpsPosition() { }

    private TGpsPosition(
        Guid vehicleId,
        int deviceId,
        DateTime recordedAt,
        Point geometry,
        decimal? speedKmh,
        short? headingDeg,
        bool? ignition,
        decimal? odometerKm,
        decimal? hdop,
        short? satellites)
    {
        VehicleId = vehicleId;
        DeviceId = deviceId;
        RecordedAt = recordedAt;
        ReceivedAt = DateTime.UtcNow;
        Geometry = geometry;
        SpeedKmh = speedKmh;
        HeadingDeg = headingDeg;
        Ignition = ignition;
        OdometerKm = odometerKm;
        Hdop = hdop;
        Satellites = satellites;
    }

    public static TGpsPosition Create(
        Guid vehicleId,
        int deviceId,
        DateTime recordedAt,
        Point geometry,
        decimal? speedKmh,
        short? headingDeg,
        bool? ignition,
        decimal? odometerKm,
        decimal? hdop,
        short? satellites)
    {
        return new TGpsPosition(
            vehicleId,
            deviceId,
            recordedAt,
            geometry,
            speedKmh,
            headingDeg,
            ignition,
            odometerKm,
            hdop,
            satellites);
    }
}