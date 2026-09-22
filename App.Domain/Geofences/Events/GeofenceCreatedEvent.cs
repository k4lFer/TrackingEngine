using App.Shared.Domain;

namespace App.Domain.Geofences.Events;

public sealed record GeofenceCreatedEvent(Guid GeofenceId, string Code, string Name) : BaseEvent;