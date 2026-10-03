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

/// <summary>
/// Worker que marca como Offline los vehículos que dejaron de reportar.
///
/// POR QUÉ UN BackgroundService Y NO UN INTERCEPTOR. El repo despacha los eventos de
/// dominio desde DomainEventDispatcherInterceptor, que es un SaveChangesInterceptor:
/// solo se ejecuta cuando alguien llama a SaveChanges. Un vehículo que se apagó no
/// genera ninguna escritura, así que el interceptor no tiene nada a lo que colgarse.
/// "No ha pasado nada en 5 minutos" es la ausencia de escrituras, y no se puede
/// detectar la ausencia de un evento con un hook sobre el evento. Hace falta un
/// reloj, y el reloj es un BackgroundService.
///
/// CÓMO EVITA DUPLICADOS. La consulta filtra por State != Offline, así que un
/// vehículo marcado deja de aparecer en las pasadas siguientes y no se repite el
/// evento. Idempotente por construcción.
///
/// POR QUÉ ExecuteUpdateAsync. Va directo a la base, sin materializar las entidades y
/// sin disparar el interceptor de eventos de dominio, que es justo lo que se quiere:
/// un UPDATE masivo de "sin señal" no debe generar 300 eventos VehiclePositionReported.
///
/// A propósito NO usa ResetToOffline(): ese método pone LastReceivedAt en null, y
/// perderíamos el "sin señal desde hace 3 h" que el tablero necesita mostrar.
public sealed class VehicleStaleMonitorHostedService : BackgroundService
{
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

        // La primera pasada espera un ciclo, no arranca en el mismo momento que el
        // host: al arrancar no hay ninguna trama recibida todavía y marcaría toda la
        // flota como sin señal.
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

        var cutoff = DateTime.UtcNow - _offlineThreshold;

        // Se proyecta a un tipo anónimo en vez de materializar TVehicleCurrentState:
        // solo hace falta el id, la hora y el último punto para el evento.
        var stale = await db.VehicleCurrentStates
            .Where(s => s.State != VehicleState.Offline && s.LastReceivedAt < cutoff)
            .Select(s => new { s.VehicleId, s.ActiveTripId, s.CurrentGeofenceId, s.LastGeom, s.LastReceivedAt, s.StoppedSince })
            .ToListAsync(cancellationToken);

        if (stale.Count == 0) return;

        var affected = 0;

        foreach (var vehicle in stale)
        {
            // El filtro de LastReceivedAt < cutoff se REPITE en el UPDATE, y no solo en
            // la consulta de arriba. Sin esa repetición hay una ventana entre el SELECT
            // y el UPDATE: si en medio llega una trama, el vehículo deja de estar
            // "stale" pero su State sigue sin ser Offline, así que el UPDATE lo
            // marcaba Offline igualmente y emitía un GpsConnectionLost que no era
            // cierto. ExecuteUpdateAsync devuelve las filas afectadas de verdad, que es
            // lo que permite distinguir "lo marqué yo" de "ya no estaba stale".
            var rows = await db.VehicleCurrentStates
                .Where(s => s.VehicleId == vehicle.VehicleId
                         && s.State != VehicleState.Offline
                         && s.LastReceivedAt < cutoff)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(s => s.State, VehicleState.Offline)
                        .SetProperty(s => s.UpdatedAt, DateTime.UtcNow)
                        // Se cierra la parada en curso: si el vehículo muere figurando
                        // "Detenido" y no se limpia, el tablero acaba mostrando
                        // "detenido hace 8 días", que es mentira.
                        .SetProperty(s => s.StoppedSince, (DateTime?)null)
                        .SetProperty(s => s.LongStopNotifiedAt, (DateTime?)null),
                    cancellationToken);

            if (rows == 0) continue;

            affected++;

            var payload = JsonSerializer.Serialize(new
            {
                last_received_at = vehicle.LastReceivedAt,
                offline_after_s = (int)_offlineThreshold.TotalSeconds,
            });

            // Add + SaveChanges: ExecuteUpdateAsync no pasa por el ChangeTracker, así que
            // el evento hay que insertarlo a mano. Sin este SaveChanges el evento se
            // quedaba en memoria y se perdía al liberar el scope.
            db.TrackingEvents.Add(TTrackingEvent.Create(
                vehicle.VehicleId,
                vehicle.ActiveTripId,
                TrackingEventType.GpsConnectionLost,
                EventSeverity.Warning,
                vehicle.CurrentGeofenceId,
                vehicle.LastGeom,
                payload));
        }

        if (affected == 0) return;

        await db.SaveChangesAsync(cancellationToken);

        _logger.LogWarning(
            "{Count} vehículo(s) marcados sin señal. Última trama recibida hace más de {Threshold}.",
            affected, _offlineThreshold);

        await notifier.DataChangedAsync(cancellationToken);
    }
}

