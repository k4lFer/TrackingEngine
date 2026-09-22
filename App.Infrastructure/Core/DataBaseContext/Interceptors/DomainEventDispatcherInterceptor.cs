using System.Collections.Concurrent;
using System.Reflection;
using App.Infrastructure.Core.DataBaseContext.Audit;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Shared.Domain;
using Cortex.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Core.DataBaseContext.Interceptors;

public sealed class DomainEventDispatcherInterceptor : SaveChangesInterceptor
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> PublishCache = new();
    private static readonly MethodInfo PublishMethodDef = typeof(IMediator)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .First(m => m.Name == nameof(IMediator.PublishAsync)
                 && m.IsGenericMethodDefinition
                 && m.GetParameters().Length == 2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AsyncLocal<List<(BaseEvent Event, Guid AuditId)>?> _currentPayload = new();

    public DomainEventDispatcherInterceptor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CaptureEvents(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureEvents(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CaptureEvents(DbContext? context)
    {
        if (context is null)
            return;

        var entries = context.ChangeTracker
            .Entries<BaseDomain>()
            .Where(x => x.Entity.DomainEvents.Count != 0)
            .ToArray();

        if (entries.Length == 0)
            return;

        var payload = new List<(BaseEvent, Guid)>();

        foreach (var entry in entries)
        {
            var events = entry.Entity.DomainEvents.ToList();
            entry.Entity.ClearDomainEvents();

            foreach (var @event in events)
            {
                var audit = new AuditMessage(@event);
                context.Set<AuditMessage>().Add(audit);
                payload.Add((@event, audit.Id));
            }
        }

        _currentPayload.Value = payload;
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        DispatchEventsAsync(default).GetAwaiter().GetResult();
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await DispatchEventsAsync(cancellationToken);
        return result;
    }

    private async Task DispatchEventsAsync(CancellationToken cancellationToken)
    {
        var payload = _currentPayload.Value;
        _currentPayload.Value = null;

        if (payload is null || payload.Count == 0)
            return;

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DomainEventDispatcherInterceptor>>();

        foreach (var (@event, auditId) in payload)
        {
            try
            {
                var eventType = @event.GetType();
                var method = PublishCache.GetOrAdd(eventType, t => PublishMethodDef.MakeGenericMethod(t));
                await (Task)method.Invoke(mediator, [@event, cancellationToken])!;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Event {EventType} publish failed. AuditId: {AuditId}", @event.GetType().Name, auditId);

                try
                {
                    using var ctxScope = _scopeFactory.CreateScope();
                    var ctx = ctxScope.ServiceProvider.GetRequiredService<AppDataBaseContext>();
                    var audit = await ctx.AuditMessages.FindAsync([auditId], cancellationToken);
                    if (audit is not null)
                    {
                        audit.Error = $"{ex.GetType().Name}: {ex.Message}";
                        await ctx.SaveChangesAsync(cancellationToken);
                    }
                }
                catch (Exception inner)
                {
                    logger.LogError(inner, "Failed to persist audit error for {AuditId}", auditId);
                }
            }
        }
    }
}
