using System.Collections.Concurrent;
using System.Reflection;
using App.Shared.Domain;
using App.Shared.Security;
using Cortex.Mediator;

namespace App.Infrastructure.Core.Services;

public class EventBus : IEventBus
{
    private readonly IMediator _mediator;
    private static readonly ConcurrentDictionary<Type, MethodInfo> PublishCache = new();
    private static readonly MethodInfo PublishMethodDef = typeof(IMediator)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .First(m => m.Name == nameof(IMediator.PublishAsync)
                 && m.IsGenericMethodDefinition
                 && m.GetParameters().Length == 2);

    public EventBus(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task PublishAndClearAsync(BaseDomain entity, CancellationToken cancellationToken = default)
    {
        var events = entity.DomainEvents.ToList();
        entity.ClearDomainEvents();

        foreach (var @event in events)
        {
            await PublishAsync(@event, cancellationToken);
        }
    }

    public async Task PublishAsync(BaseEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType();
        var method = PublishCache.GetOrAdd(eventType, t => PublishMethodDef.MakeGenericMethod(t));
        await (Task)method.Invoke(_mediator, [domainEvent, cancellationToken])!;
    }
}
