using App.Interfaces.Ports.Tracking;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Simulation.DTOs.Output.Response;
using App.Objects.Tracking.DTOs.Input.Command;
using App.UseCases.Tracking.Command.ReportPosition;
using Cortex.Mediator;
using Microsoft.Extensions.Logging;

namespace App.UseCases.Gps;

/// <summary>Resultado del procesamiento de una trama GPS entrante.</summary>
public sealed record GpsIngestResult(
    string Transport,
    string? Decoder,
    int Accepted,
    int Skipped);

/// <summary>
/// Servicio de ingesta genérico: recibe una trama en crudo, intenta cada
/// decodificador registrado y, por cada lectura, resuelve el vehículo por su
/// DeviceId y reenvía a la pipeline estándar (ReportPositionCommand + SignalR).
/// </summary>
public sealed class GpsIngestionService
{
    private readonly IReadOnlyList<IGpsDeviceDecoder> _decoders;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IMediator _mediator;
    private readonly ITrackingNotifier _notifier;
    private readonly ILogger<GpsIngestionService> _logger;

    public GpsIngestionService(
        IEnumerable<IGpsDeviceDecoder> decoders,
        IVehicleRepository vehicleRepository,
        IMediator mediator,
        ITrackingNotifier notifier,
        ILogger<GpsIngestionService> logger)
    {
        _decoders = decoders.ToList();
        _vehicleRepository = vehicleRepository;
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
            foreach (var reading in readings)
            {
                var vehicle = await _vehicleRepository.GetByDeviceIdentifierAsync(reading.DeviceId, cancellationToken);
                if (vehicle is null)
                {
                    _logger.LogWarning("[gps:{Transport}/{Decoder}] dispositivo {DeviceId} no vinculado a ningún vehículo", transport, decoder.Name, reading.DeviceId);
                    skipped++;
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

            return new GpsIngestResult(transport, decoder.Name, accepted, skipped);
        }

        _logger.LogWarning("[gps:{Transport}] trama no reconocida por ningún protocolo", transport);
        return new GpsIngestResult(transport, Decoder: null, 0, 0);
    }
}