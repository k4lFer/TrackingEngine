using App.Shared.Domain;

namespace App.Domain.Vehicles.Events;

public sealed record VehicleUpdatedEvent(Guid VehicleId, string Code, string Plate, string Brand, string Model, bool Active) : BaseEvent;