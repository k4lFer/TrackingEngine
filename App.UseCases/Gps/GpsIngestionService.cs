using App.Domain.Positions.Entities;
using App.Interfaces.Ports.Tracking;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Gps.DTOs;
using App.Objects.Gps.DTOs.Output.Response;
using App.Shared.Common.Enums;
using App.Shared.Common.Gps;
using App.UseCases.Gps.Caching;
using Microsoft.Extensions.Logging;

namespace App.UseCases.Gps;

public sealed class GpsIngestionService
{
    private static readonly SemaphoreSlim StagingWriteGate = new(64, 64);

    private readonly IReadOnlyList<IGpsDeviceDecoder> _decoders;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleRealtimeGate _gate;
    private readonly ITrackingNotifier _notifier;
    private readonly ILogger<GpsIngestionService> _logger;
    private readonly IGpsIngestCache _cache;
    private readonly IGpsStagingRepository _staging;

    public GpsIngestionService(
        IEnumerable<IGpsDeviceDecoder> decoders,
        IVehicleRepository vehicleRepository,
        IVehicleRealtimeGate gate,
        ITrackingNotifier notifier,
        ILogger<GpsIngestionService> logger,
        IGpsIngestCache cache,
        IGpsStagingRepository staging)
    {
        _decoders = decoders.ToList();
        _vehicleRepository = vehicleRepository;
        _gate = gate;
        _notifier = notifier;
        _logger = logger;
        _cache = cache;
        _staging = staging;
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
                switch (await TryProcessReadingAsync(transport, decoder, reading, cancellationToken))
                {
                    case ReadingOutcome.Accepted: accepted++; break;
                    case ReadingOutcome.Skipped: skipped++; break;
                    case ReadingOutcome.Suppressed: suppressed++; break;
                }
            }

            return new GpsIngestResult(transport, decoder.Name, accepted, skipped, suppressed);
        }

        _logger.LogWarning("[gps:{Transport}] trama no reconocida por ningún protocolo", transport);
        return new GpsIngestResult(transport, Decoder: null, 0, 0, 0);
    }

    private async Task<ReadingOutcome> TryProcessReadingAsync(
        string transport,
        IGpsDeviceDecoder decoder,
        DeviceGpsReading reading,
        CancellationToken cancellationToken)
    {
        if (!_cache.TryGetVehicleId(reading.DeviceId, out var vehicleId))
        {
            var vehicle = await _vehicleRepository.GetIngestSnapshotByDeviceIdentifierAsync(reading.DeviceId, cancellationToken);
            if (vehicle is null)
            {
                _logger.LogWarning("[gps:{Transport}/{Decoder}] dispositivo {DeviceId} no vinculado a ningún vehículo", transport, decoder.Name, reading.DeviceId);
                return ReadingOutcome.Skipped;
            }
            vehicleId = vehicle.Id;
            _cache.SetVehicleId(reading.DeviceId, vehicleId);
        }

        var now = DateTime.UtcNow;

        var recordedAt = reading.RecordedAt ?? now;
        if (_cache.ShouldSkipByRecordedAt(vehicleId, recordedAt, TimeSpan.Zero))
        {
            _logger.LogDebug("[gps:{Transport}/{Decoder}] trama deduplicada por recordedAt para vehicleId {VehicleId}", transport, decoder.Name, vehicleId);
            return ReadingOutcome.Skipped;
        }

        var decision = _gate.Evaluate(vehicleId, reading.Latitude, reading.Longitude, reading.SpeedKmh, now);
        if (decision == GpsGateDecision.Suppressed)
        {
            _logger.LogDebug("[gps:{Transport}/{Decoder}] trama suprimida para vehicleId {VehicleId}: punto repetido", transport, decoder.Name, vehicleId);
            await SendHeartbeatAsync(vehicleId, now, cancellationToken);
            return ReadingOutcome.Suppressed;
        }

        if (reading.Latitude is not (>= -90 and <= 90) ||
            reading.Longitude is not (>= -180 and <= 180) ||
            (reading.Latitude == 0 && reading.Longitude == 0))
        {
            _logger.LogWarning("[gps:{Transport}/{Decoder}] coordenadas inválidas para vehicleId {VehicleId}", transport, decoder.Name, vehicleId);
            return ReadingOutcome.Skipped;
        }

        await StagingWriteGate.WaitAsync(cancellationToken);
        try
        {
            await _staging.SaveAsync(new TGpsPositionStaging
            {
                DeviceId = reading.DeviceId,
                VehicleId = vehicleId,
                Latitude = reading.Latitude,
                Longitude = reading.Longitude,
                SpeedKmh = reading.SpeedKmh.HasValue ? (double?)reading.SpeedKmh.Value : null,
                HeadingDeg = reading.HeadingDeg.HasValue ? (double?)reading.HeadingDeg.Value : null,
                Satellites = reading.Satellites,
                Hdop = reading.Hdop.HasValue ? (double?)reading.Hdop.Value : null,
                Ignition = reading.Ignition,
                DeviceTimestamp = recordedAt,
                ReceivedAt = now,
                Transport = transport,
                Decoder = decoder.Name
            }, cancellationToken);
        }
        finally
        {
            StagingWriteGate.Release();
        }

        // El dedupe del hot path avanza solo tras persistir: si el INSERT
        // fallara antes, un reintento del mismo timestamp no se descartaría.
        _cache.UpdateLastRecordedAt(vehicleId, recordedAt);
        return ReadingOutcome.Accepted;
    }

    private enum ReadingOutcome { Accepted, Skipped, Suppressed }

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
        if (!_gate.TryClaimHeartbeat(vehicleId, now)) return;

        try
        {
            await _vehicleRepository.TouchLastReceivedAsync(vehicleId, now, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[gps] no se pudo refrescar la última señal del vehículo {VehicleId}", vehicleId);
            return;
        }

        if (_gate.TryClaimHeartbeatBroadcast(now))
        {
            await _notifier.DataChangedAsync(cancellationToken);
        }
    }
}