using App.Shared.Domain;

namespace App.Domain.Tracking.Events;

public sealed record TripCreatedEvent(Guid TripId, Guid VehicleId) : BaseEvent;