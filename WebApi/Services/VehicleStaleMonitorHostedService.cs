using System.Text.Json;
using App.Domain.Tracking.Entities;
using App.Domain.Vehicles.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Tracking;
using App.Shared.Common.Enums;
using App.Shared.Common.Gps;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace WebApi.Services;

public sealed class VehicleStaleMonitorHostedService : BackgroundService
{
    private const int BatchSize = 500;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VehicleStaleMonitorHostedService> _logger;
    private readonly TimeSpan _scanInterval;
    private readonly TimeSpan _offlineThreshold;

    public VehicleStaleMonitorHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<GpsRealtimeOptions> options,
        ILogger<VehicleStaleMonitorHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _scanInterval = TimeSpan.FromSeconds(Math.Max(5, options.Value.StaleScanIntervalSeconds));
        _offlineThreshold = TimeSpan.FromSeconds(Math.Max(30, options.Value.OfflineThresholdSeconds));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Monitor de vehículos sin señal activo: corte {Threshold}, revisión cada {Interval}.",
            _offlineThreshold, _scanInterval);

        using var timer = new PeriodicTimer(_scanInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SweepAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                // Una pasada fallida no puede tumbar el worker: se registra y sigue.
                _logger.LogError(ex, "Fallo en la revisión de vehículos sin señal");
            }
        }
    }

    private async Task SweepAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDataBaseContext>();
        var notifier = scope.ServiceProvider.GetRequiredService<ITrackingNotifier>();

        var now = DateTime.UtcNow;
        var cutoff = now - _offlineThreshold;

        var candidates = await db.VehicleCurrentStates
            .Where(s => s.State != VehicleState.Offline && s.LastReceivedAt < cutoff)
            .Select(s => new { s.VehicleId, s.ActiveTripId, s.CurrentGeofenceId, s.LastGeom, s.LastReceivedAt })
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0) return;

        var candidatesById = candidates.ToDictionary(c => c.VehicleId);
        var flipped = new HashSet<Guid>();

        foreach (var ids in Chunks(candidatesById.Keys, BatchSize))
        {

            await db.VehicleCurrentStates
                .Where(s => ids.Contains(s.VehicleId)
                         && s.State != VehicleState.Offline
                         && s.LastReceivedAt < cutoff)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(s => s.State, VehicleState.Offline)
                        .SetProperty(s => s.UpdatedAt, now)
                        .SetProperty(s => s.StoppedSince, (DateTime?)null)
                        .SetProperty(s => s.LongStopNotifiedAt, (DateTime?)null),
                    cancellationToken);

            var justFlipped = await db.VehicleCurrentStates
                .Where(s => ids.Contains(s.VehicleId)
                         && s.State == VehicleState.Offline
                         && s.LastReceivedAt < cutoff)
                .Select(s => s.VehicleId)
                .ToListAsync(cancellationToken);

            foreach (var id in justFlipped) flipped.Add(id);
        }

        if (flipped.Count == 0) return;

        var events = new List<TTrackingEvent>(flipped.Count);

        foreach (var id in flipped)
        {
            var vehicle = candidatesById[id];

            var payload = JsonSerializer.Serialize(new
            {
                last_received_at = vehicle.LastReceivedAt,
                offline_after_s = (int)_offlineThreshold.TotalSeconds,
            });

            events.Add(TTrackingEvent.Create(
                vehicle.VehicleId,
                vehicle.ActiveTripId,
                TrackingEventType.GpsConnectionLost,
                EventSeverity.Warning,
                vehicle.CurrentGeofenceId,
                vehicle.LastGeom,
                payload));
        }

        db.TrackingEvents.AddRange(events);
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogWarning(
            "{Count} vehículo(s) marcados sin señal de {Candidates} candidato(s). Última trama recibida hace más de {Threshold}.",
            flipped.Count, candidates.Count, _offlineThreshold);

        await notifier.DataChangedAsync(cancellationToken);
    }

    private static List<T[]> Chunks<T>(ICollection<T> source, int size)
    {
        var result = new List<T[]>();
        var buffer = new T[size];
        var count = 0;

        foreach (var item in source)
        {
            buffer[count++] = item;
            if (count == size)
            {
                result.Add(buffer);
                buffer = new T[size];
                count = 0;
            }
        }

        if (count > 0) result.Add(buffer[..count]);

        return result;
    }
}

