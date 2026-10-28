using App.Objects.Gps.Events;

namespace App.Interfaces.Ports.Messaging;

public interface IGpsEventPublisher
{
    Task PublishAsync(GpsPositionReceived @event, CancellationToken cancellationToken = default);
}
