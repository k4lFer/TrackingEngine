using Cortex.Mediator.Notifications;

namespace App.Shared.Common.Domain;

public abstract record BaseEvent : INotification
{
    public DateTime DateOccurred { get; init; } = DateTime.UtcNow;
}