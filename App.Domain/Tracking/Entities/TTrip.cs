using App.Domain.Tracking.Events;
using App.Shared.Domain;
using App.Shared.Objects.Enums;
using NetTopologySuite.Geometries;

namespace App.Domain.Tracking.Entities;

public class TTrip : BaseDomain
{
    public Guid VehicleId { get; private set; }
    public Guid? OriginGeofenceId { get; private set; }
    public Guid? DestinationGeofenceId { get; private set; }
    public Guid? RouteId { get; private set; }
    public Guid? MaterialId { get; private set; }
    public decimal? LoadTonnes { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public decimal? DistanceKm { get; private set; }
    public int? DurationS { get; private set; }
    public decimal? MaxSpeedKmh { get; private set; }
    public int DeviationCount { get; private set; }
    public int SpeedAlertCount { get; private set; }
    public TripStatus Status { get; private set; } = TripStatus.Active;
    public LineString? Track { get; private set; }

    private TTrip() { }

    private TTrip(
        Guid vehicleId,
        Guid? originGeofenceId,
        Guid? destinationGeofenceId,
        Guid? routeId,
        Guid? materialId)
    {
        VehicleId = vehicleId;
        OriginGeofenceId = originGeofenceId;
        DestinationGeofenceId = destinationGeofenceId;
        RouteId = routeId;
        MaterialId = materialId;
        StartedAt = DateTime.UtcNow;
        Status = TripStatus.Active;
    }

    public static TTrip Start(
        Guid vehicleId,
        Guid? originGeofenceId,
        Guid? destinationGeofenceId,
        Guid? routeId,
        Guid? materialId,
        decimal? loadTonnes = null)
    {
        var t = new TTrip(vehicleId, originGeofenceId, destinationGeofenceId, routeId, materialId)
        {
            LoadTonnes = loadTonnes
        };
        t.AddDomainEvent(new TripCreatedEvent(t.Id, t.VehicleId));
        return t;
    }

    /// <summary>Reinicia el reloj del viaje al momento actual (migración de operaciones del simulador).</summary>
    public void RestartClock() => StartedAt = DateTime.UtcNow;

    public void Complete(
        decimal? distanceKm,
        int? durationS,
        decimal? maxSpeedKmh,
        LineString? track,
        TripStatus status)
    {
        DistanceKm = distanceKm;
        DurationS = durationS;
        MaxSpeedKmh = maxSpeedKmh;
        Track = track;
        Status = status;
        EndedAt = DateTime.UtcNow;
    }

    public void AddDeviation() => DeviationCount++;
    public void AddSpeedAlert() => SpeedAlertCount++;
}