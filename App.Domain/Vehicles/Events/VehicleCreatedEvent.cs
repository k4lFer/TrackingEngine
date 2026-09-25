using App.Shared.Common.Domain;

namespace App.Domain.Vehicles.Events;

public sealed record VehicleCreatedEvent(Guid VehicleId, string Code, string Plate, string Brand, string Model) : BaseEvent;