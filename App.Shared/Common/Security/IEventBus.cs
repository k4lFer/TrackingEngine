using App.Shared.Common.Domain;

namespace App.Shared.Common.Security;

public interface IEventBus
{
    Task PublishAsync(BaseEvent domainEvent, CancellationToken cancellationToken = default);
    Task PublishAndClearAsync(BaseDomain entity, CancellationToken cancellationToken = default);
}