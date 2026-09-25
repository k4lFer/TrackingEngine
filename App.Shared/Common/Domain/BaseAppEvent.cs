using Cortex.Mediator.Notifications;

namespace App.Shared.Common.Domain;

public abstract record BaseAppEvent : INotification
{
    public DateTime DateOccurred { get; init; } = DateTime.UtcNow;
}
