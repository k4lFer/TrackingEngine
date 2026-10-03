using App.Interfaces.Ports.Tracking;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Simulation.DTOs.Output.Response;
using App.Objects.Tracking.DTOs.Input.Command;
using App.Shared.Common.Enums;
using App.Shared.Common.Gps;
using App.UseCases.Tracking.Command.ReportPosition;
using Cortex.Mediator;
using Microsoft.Extensions.Logging;

namespace App.UseCases.Gps;

/// <summary>
/// Resultado del procesamiento de una trama GPS entrante.
/// </summary>
/// <param name="Accepted">Tramas que pasaron el filtro y llegaron al pipeline.</param>
/// <param name="Skipped">Tramas que no aportan nada: dispositivo sin vehículo, o
/// posición rechazada por el pipeline.</param>
/// <param name="Suppressed">Tramas que el filtro de repetidas retuvo por ser un punto
/// ya conocido. Antes iban a parar a <c>Skipped</c>, que mezclaba dos cosas muy
/// distintas: una trama perdida y una trama deliberadamente ignorada. Un operador
/// leyendo el log veía "skipped" y buscaba un error donde no lo había.</param>
public sealed record GpsIngestResult(
    string Transport,
    string? Decoder,
    int Accepted,
    int Skipped,
    int Suppressed = 0);

/// <summary>
/// Servicio de ingesta genérico: recibe una trama en crudo, intenta cada
/// decodificador registrado y, por cada lectura, resuelve el vehículo por su
/// DeviceId y reenvía a la pipeline estándar (ReportPositionCommand + SignalR).
///
/// Dos cortes reducen la carga, ambos configurables en la sección "GpsRealtime":
/// el <see cref="IVehicleRealtimeGate"/> descarta tramas repetidas de vehículos
/// detenidos, y para las que descarta se refresca "última señal" con un latido
/// coalescado, para que el tablero no congele ese dato.
/// </summary>
public sealed class GpsIngestionService
{
    private readonly IReadOnlyList<IGpsDeviceDecoder> _decoders;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleRealtimeGate _gate;
    private readonly IMediator _mediator;
    private readonly ITrackingNotifier _notifier;
    private readonly ILogger<GpsIngestionService> _logger;

    public GpsIngestionService(
        IEnumerable<IGpsDeviceDecoder> decoders,
        IVehicleRepository vehicleRepository,
        IVehicleRealtimeGate gate,
        IMediator mediator,
        ITrackingNotifier notifier,
        ILogger<GpsIngestionService> logger)
    {
        _decoders = decoders.ToList();
        _vehicleRepository = vehicleRepository;
        _gate = gate;
        _mediator = mediator;
        _notifier = notifier;
        _logger = logger;
    }


    public async Task<GpsIngestResult> HandleAsync(
        string transport,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken = default)
    {
        foreach (var decoder in _decoders)
        {
            if (!decoder.CanDecode(payload.Span)) continue;
            if (!decoder.TryDecode(payload.Span, out var readings)) continue;

            var accepted = 0;
            var skipped = 0;
            var suppressed = 0;
            foreach (var reading in readings)
            {
                var vehicle = await _vehicleRepository.GetByDeviceIdentifierAsync(reading.DeviceId, cancellationToken);
                if (vehicle is null)
                {
                    _logger.LogWarning("[gps:{Transport}/{Decoder}] dispositivo {DeviceId} no vinculado a ningún vehículo", transport, decoder.Name, reading.DeviceId);
                    skipped++;
                    continue;
                }

                var now = DateTime.UtcNow;

                var decision = _gate.Evaluate(vehicle.Id, reading.Latitude, reading.Longitude, reading.SpeedKmh, now);
                if (decision == GpsGateDecision.Suppressed)
                {
                    _logger.LogDebug("[gps:{Transport}/{Decoder}] trama suprimida para vehículo {VehicleCode}: punto repetido", transport, decoder.Name, vehicle.Code);
                    await SendHeartbeatAsync(vehicle.Id, now, cancellationToken);
                    suppressed++;
                    continue;
                }

                var request = new PositionReportRequest
                {
                    VehicleId = vehicle.Id,
                    DeviceId = reading.DeviceId,
                    Latitude = reading.Latitude,
                    Longitude = reading.Longitude,
                    RecordedAt = reading.RecordedAt ?? DateTime.UtcNow,
                    SpeedKmh = reading.SpeedKmh,
                    HeadingDeg = reading.HeadingDeg,
                    Hdop = reading.Hdop,
                    Satellites = reading.Satellites,
                    Ignition = reading.Ignition,
                };

                var result = await _mediator.SendCommandAsync(new ReportPositionCommand(request), cancellationToken);
                if (result.StatusCode is not (System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Created))
                {
                    _logger.LogWarning("[gps:{Transport}/{Decoder}] posición rechazada para vehículo {VehicleCode}: {Status}", transport, decoder.Name, vehicle.Code, (int)result.StatusCode);
                    skipped++;
                    continue;
                }

                accepted++;
                await _notifier.VehicleMovedAsync(new VehicleLiveDto(
                    vehicle.Id,
                    vehicle.Code,
                    vehicle.CurrentState?.ActiveTripId,
                    reading.Latitude,
                    reading.Longitude,
                    reading.SpeedKmh ?? 0,
                    reading.HeadingDeg ?? 0,
                    DistanceKm: 0,
                    Progress: 0,
                    EtaSec: 0,
                    null, null, null, null,
                    LastUpdated: reading.RecordedAt ?? DateTime.UtcNow), cancellationToken);
            }

            if (accepted > 0)
            {
                await _notifier.DataChangedAsync(cancellationToken);
            }

            return new GpsIngestResult(transport, decoder.Name, accepted, skipped, suppressed);
        }

        _logger.LogWarning("[gps:{Transport}] trama no reconocida por ningún protocolo", transport);
        return new GpsIngestResult(transport, Decoder: null, 0, 0, 0);
    }

    /// <summary>
    /// Refresca "última señal" de un vehículo cuya trama fue suprimida. Sin esto la
    /// columna se quedaría congelada en la hora del último punto guardado durante
    /// toda la parada, y en /fleet el vehículo parecería sin señal.
    ///
    /// Va con un UPDATE directo (ExecuteUpdateAsync) en vez de pasar por el pipeline
    /// completo, y con un refresco global a los clientes coalescado.
    /// </summary>
    private async Task SendHeartbeatAsync(Guid vehicleId, DateTime now, CancellationToken cancellationToken)
    {
        // El control de ventana vive en el gate (singleton) y no aquí: este servicio
        // es Scoped y el gateway crea un scope por trama, así que un contador local
        // arrancaría en cero en cada envío y refrescaría la señal en todas.
        if (!_gate.TryClaimHeartbeat(vehicleId, now)) return;

        try
        {
            await _vehicleRepository.TouchLastReceivedAsync(vehicleId, now, cancellationToken);
        }
        catch (Exception ex)
        {
            // Un latido fallido no puede tumbar la ingesta: se registra y se sigue.
            _logger.LogWarning(ex, "[gps] no se pudo refrescar la última señal del vehículo {VehicleId}", vehicleId);
            return;
        }

        if (_gate.TryClaimHeartbeatBroadcast(now))
        {
            await _notifier.DataChangedAsync(cancellationToken);
        }
    }
}