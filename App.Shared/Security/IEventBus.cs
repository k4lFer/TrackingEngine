using App.Shared.Domain;

namespace App.Shared.Security;

public interface IEventBus
{
    Task PublishAsync(BaseEvent domainEvent, CancellationToken cancellationToken = default);
    Task PublishAndClearAsync(BaseDomain entity, CancellationToken cancellationToken = default);
}