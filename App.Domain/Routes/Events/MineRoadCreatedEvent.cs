using App.Shared.Common.Domain;

namespace App.Domain.Routes.Events;

public sealed record MineRoadCreatedEvent(Guid MineRoadId, string Code, string Name) : BaseEvent;