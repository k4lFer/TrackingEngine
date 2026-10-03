using App.Domain.Vehicles.Events;
using App.Shared.Common.Domain;
using App.Shared.Common.Enums;
using NetTopologySuite.Geometries;

namespace App.Domain.Vehicles.Entities;

public class TVehicleCurrentState : BaseDomain
{
    public Guid VehicleId { get; private set; }
    public VehicleState State { get; private set; } = VehicleState.Offline;
    public DateTime? LastReportedAt { get; private set; }
    public DateTime? LastReceivedAt { get; private set; }
    public Point? LastGeom { get; private set; }
    public Guid? CurrentGeofenceId { get; private set; }
    public Guid? ActiveTripId { get; private set; }
    public TVehicle? Vehicle { get; private set; }
    public DateTime? OverSpeedSince { get; private set; }
    public DateTime? OffRouteSince { get; private set; }
    public int OffRouteStreak { get; private set; }
    public DateTime? StoppedSince { get; private set; }
    public DateTime? LongStopNotifiedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private TVehicleCurrentState() { }

    public static TVehicleCurrentState Create(Guid vehicleId)
    {
        var st = new TVehicleCurrentState { VehicleId = vehicleId };
        return st;
    }

    public void ReportPosition(Point geom, VehicleState state, DateTime reportedAt)
    {
        var stateChanged = State != state;

        LastGeom = geom;
        State = state;
        LastReportedAt = reportedAt;
        LastReceivedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        if (state == VehicleState.Detenido)
        {
            if (stateChanged)
            {
                StoppedSince = LastReceivedAt;
                LongStopNotifiedAt = null;
            }
        }
        else if (StoppedSince is not null)
        {
            StoppedSince = null;
            LongStopNotifiedAt = null;
        }

        if (stateChanged)
        {
            AddDomainEvent(new VehiclePositionReportedEvent(VehicleId, geom, state));
        }
    }

    public void MarkLongStopNotified() => LongStopNotifiedAt = DateTime.UtcNow;

    public void SetActiveTrip(Guid? tripId)
    {
        ActiveTripId = tripId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyGeofence(Guid? geofenceId)
    {
        CurrentGeofenceId = geofenceId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkOverspeed(DateTime at)
    {
        OverSpeedSince = at;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearOverspeed()
    {
        OverSpeedSince = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddOffRouteStreak() => OffRouteStreak++;

    public void MarkOffRoute(DateTime at)
    {
        OffRouteSince = at;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearOffRoute()
    {
        OffRouteSince = null;
        OffRouteStreak = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetToOffline()
    {
        State = VehicleState.Offline;
        LastGeom = null;
        LastReportedAt = null;
        LastReceivedAt = null;
        CurrentGeofenceId = null;
        ActiveTripId = null;
        StoppedSince = null;
        LongStopNotifiedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}