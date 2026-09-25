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

    /// <summary>Inicio del episodio de sobrevelocidad (histeresis: solo se emite al cruzar el límite hacia arriba).</summary>
    public DateTime? OverSpeedSince { get; private set; }

    /// <summary>Inicio del episodio de desvío de ruta (se exige un mínimo de reportes consecutivos fuera de ruta).</summary>
    public DateTime? OffRouteSince { get; private set; }

    public int OffRouteStreak { get; private set; }

    /// <summary>Momento en que el vehículo entró en Detenido. Se limpia al salir.</summary>
    public DateTime? StoppedSince { get; private set; }

    /// <summary>Momento en que se ya notificó la parada larga de este episodio. Evita duplicar el evento.</summary>
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

        // El tiempo de parada se mide con el reloj del servidor: RecordedAt viene del
        // dispositivo (el TK103 solo tiene resolución de 1 segundo) y no es fiable.
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

        // El evento significa "el vehículo cambió de estado", no "llegó una posición".
        // Emitirlo en cada trama costaba un INSERT en audit_messages más un scope de DI
        // y un PublishAsync por reflexión en cada commit, sin ningún consumidor.
        if (stateChanged)
        {
            AddDomainEvent(new VehiclePositionReportedEvent(VehicleId, geom, state));
        }
    }

    /// <summary>Marca que la parada larga del episodio en curso ya fue notificada.</summary>
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

    /// <summary>Apaga el estado del vehículo: Offline, sin posición, sin viaje ni geofence activos.</summary>
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