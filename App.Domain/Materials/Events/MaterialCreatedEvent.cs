using App.Shared.Domain;

namespace App.Domain.Materials.Events;

public sealed record MaterialCreatedEvent(Guid MaterialId, string Code, string Name, string Unit) : BaseEvent;