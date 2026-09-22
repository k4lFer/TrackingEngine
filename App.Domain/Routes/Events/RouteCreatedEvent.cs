using App.Shared.Domain;

namespace App.Domain.Routes.Events;

public sealed record RouteCreatedEvent(
    Guid RouteId,
    string Code,
    string Name,
    Guid? OriginGeofenceId,
    Guid? DestinationGeofenceId) : BaseEvent;