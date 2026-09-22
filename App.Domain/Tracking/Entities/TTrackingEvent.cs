using App.Shared.Domain;
using App.Shared.Objects.Enums;
using NetTopologySuite.Geometries;

namespace App.Domain.Tracking.Entities;

public class TTrackingEvent : BaseDomain
{
    public Guid VehicleId { get; private set; }
    public Guid? TripId { get; private set; }
    public TrackingEventType Type { get; private set; }
    public EventSeverity Severity { get; private set; } = EventSeverity.Info;
    public DateTime OccurredAt { get; private set; }
    public Guid? GeofenceId { get; private set; }
    public Point? Position { get; private set; }
    public string? PayloadJson { get; private set; }

    public TTrip? Trip { get; private set; }

    private TTrackingEvent() { }

    private TTrackingEvent(
        Guid vehicleId,
        Guid? tripId,
        TrackingEventType type,
        EventSeverity severity,
        Guid? geofenceId,
        Point? position,
        string? payloadJson)
    {
        VehicleId = vehicleId;
        TripId = tripId;
        Type = type;
        Severity = severity;
        OccurredAt = DateTime.UtcNow;
        GeofenceId = geofenceId;
        Position = position;
        PayloadJson = payloadJson;
    }

    public static TTrackingEvent Create(
        Guid vehicleId,
        Guid? tripId,
        TrackingEventType type,
        EventSeverity severity,
        Guid? geofenceId,
        Point? position,
        string? payloadJson)
    {
        return new TTrackingEvent(vehicleId, tripId, type, severity, geofenceId, position, payloadJson);
    }
}