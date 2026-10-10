using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.Domain.Positions.Entities;
using App.Interfaces.Ports;
using App.Interfaces.Ports.Tracking;
using App.Objects.Simulation.DTOs.Output.Response;
using App.Objects.Tracking.DTOs.Input.Command;
using App.UseCases.Tracking.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.UseCases.Gps.Batching;

public sealed class GpsBatchWorker : BackgroundService
{
    private const int BatchMax = 500;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GpsBatchWorker> _logger;

    public GpsBatchWorker(IServiceScopeFactory scopeFactory, ILogger<GpsBatchWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(200));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await DrainOnceAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GpsBatchWorker: fallo procesando lote, se continúa con el siguiente");
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task DrainOnceAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var staging = scope.ServiceProvider.GetRequiredService<IGpsStagingRepository>();
        var processor = scope.ServiceProvider.GetRequiredService<IPositionProcessor>();
        var notifier = scope.ServiceProvider.GetRequiredService<ITrackingNotifier>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pending = await staging.GetPendingBatchAsync(BatchMax, ct);
        if (pending.Count == 0) return;

        var vehicleIds = pending.Where(p => p.VehicleId.HasValue)
            .Select(p => p.VehicleId!.Value).Distinct().ToList();
        var timestamps = pending.Select(p => p.DeviceTimestamp).Distinct().ToList();

        // Idempotencia: los pares (vehículo, timestamp) que ya existen en gps_positions
        // se saltan, para que un reenvío no tumbe el SaveChanges por el índice único.
        var existing = await staging.GetExistingPositionPairsAsync(vehicleIds, timestamps, ct);
        var seen = new HashSet<(Guid, DateTime)>();

        var accepted = 0;
        var backfilled = 0;
        var rejected = 0;
        var duplicates = 0;
        var liveByVehicle = new Dictionary<Guid, PositionProcessOutcome>();
        var now = DateTime.UtcNow;

        foreach (var lane in pending.Where(p => p.VehicleId.HasValue)
                     .GroupBy(p => p.VehicleId!.Value).OrderBy(g => g.Key))
        {
            foreach (var row in lane.OrderBy(p => p.DeviceTimestamp))
            {
                var vehicleId = row.VehicleId!.Value;
                var pair = (vehicleId, row.DeviceTimestamp);
                if (existing.Contains(pair) || !seen.Add(pair))
                {
                    duplicates++;
                    row.Processed = true;
                    row.ProcessedAt = now;
                    row.Error = "duplicado";
                    continue;
                }

                PositionProcessOutcome outcome;
                try
                {
                    outcome = await processor.ProcessAsync(MapToRequest(row), ct);
                }
                catch (Exception ex)
                {
                    // Falla aislada por fila: se cuarentena y se sigue con el resto.
                    rejected++;
                    row.Processed = true;
                    row.ProcessedAt = now;
                    row.Error = $"processing: {ex.Message}";
                    _logger.LogError(ex, "GpsBatchWorker: fallo aislado procesando staging {StagingId}", row.Id);
                    continue;
                }

                if (!outcome.Success)
                {
                    rejected++;
                    row.Processed = true;
                    row.ProcessedAt = now;
                    row.Error = outcome.Error;
                    continue;
                }

                accepted++;
                row.Processed = true;
                row.ProcessedAt = now;
                row.Error = null;

                if (outcome.IsBackfill)
                {
                    backfilled++;
                    continue;
                }

                liveByVehicle[vehicleId] = outcome;
            }
        }

        // Huérfanas (sin vehículo) se marcan para no bloquear la cola.
        foreach (var orphan in pending.Where(p => !p.VehicleId.HasValue))
        {
            rejected++;
            orphan.Processed = true;
            orphan.ProcessedAt = now;
            orphan.Error = "sin vehículo";
        }

        try
        {
            await unitOfWork.SaveChanges(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsBatchWorker: falló el guardado por lote ({Count} filas); reintentando fila por fila", pending.Count);
            await ProcessRowsIndividuallyAsync(pending, ct);
            return;
        }

        foreach (var outcome in liveByVehicle.Values)
        {
            await notifier.VehicleMovedAsync(BuildLiveDto(outcome), ct);
        }
        await notifier.DataChangedAsync(ct);

        _logger.LogDebug(
            "GpsBatchWorker lote guardado: {Accepted} posiciones ({Backfilled} retroactivas, {Duplicates} duplicadas, {Rejected} rechazadas, {Vehicles} vehículos)",
            accepted, backfilled, duplicates, rejected, liveByVehicle.Count);
    }

    private async Task ProcessRowsIndividuallyAsync(IReadOnlyList<TGpsPositionStaging> rows, CancellationToken ct)
    {
        foreach (var row in rows.OrderBy(p => p.ReceivedAt))
        {
            await ProcessRowAloneAsync(row.Id, ct);
        }
    }

    private async Task ProcessRowAloneAsync(Guid stagingId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var staging = scope.ServiceProvider.GetRequiredService<IGpsStagingRepository>();
        var processor = scope.ServiceProvider.GetRequiredService<IPositionProcessor>();
        var notifier = scope.ServiceProvider.GetRequiredService<ITrackingNotifier>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var row = await staging.GetPendingByIdAsync(stagingId, ct);
        if (row is null || row.Processed) return;

        try
        {
            if (row.VehicleId is not Guid vehicleId)
            {
                row.Processed = true;
                row.ProcessedAt = DateTime.UtcNow;
                row.Error = "sin vehículo";
                await unitOfWork.SaveChanges(ct);
                return;
            }

            var existing = await staging.GetExistingPositionPairsAsync(
                new[] { vehicleId }, new[] { row.DeviceTimestamp }, ct);
            if (existing.Contains((vehicleId, row.DeviceTimestamp)))
            {
                row.Processed = true;
                row.ProcessedAt = DateTime.UtcNow;
                row.Error = "duplicado";
                await unitOfWork.SaveChanges(ct);
                return;
            }

            var outcome = await processor.ProcessAsync(MapToRequest(row), ct);
            if (!outcome.Success)
            {
                row.Processed = true;
                row.ProcessedAt = DateTime.UtcNow;
                row.Error = outcome.Error;
                await unitOfWork.SaveChanges(ct);
                return;
            }

            row.Processed = true;
            row.ProcessedAt = DateTime.UtcNow;
            row.Error = null;
            await unitOfWork.SaveChanges(ct);

            if (!outcome.IsBackfill)
            {
                await notifier.VehicleMovedAsync(BuildLiveDto(outcome), ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsBatchWorker: fila {StagingId} falló individualmente; entra en cuarentena", stagingId);
            await MarkQuarantinedAsync(stagingId, ex.Message, ct);
        }

        await notifier.DataChangedAsync(ct);
    }

    private async Task MarkQuarantinedAsync(Guid stagingId, string error, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var staging = scope.ServiceProvider.GetRequiredService<IGpsStagingRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var row = await staging.GetPendingByIdAsync(stagingId, ct);
            if (row is null) return;

            row.Processed = true;
            row.ProcessedAt = DateTime.UtcNow;
            row.Error = $"cuarentena: {error}";
            await unitOfWork.SaveChanges(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GpsBatchWorker: no se pudo marcar la fila {StagingId} en cuarentena; quedará pendiente y se reintentará", stagingId);
        }
    }

    private static PositionReportRequest MapToRequest(TGpsPositionStaging row) => new()
    {
        VehicleId = row.VehicleId!.Value,
        DeviceId = row.DeviceId,
        Latitude = row.Latitude,
        Longitude = row.Longitude,
        RecordedAt = row.DeviceTimestamp,
        SpeedKmh = row.SpeedKmh.HasValue ? (decimal?)row.SpeedKmh.Value : null,
        HeadingDeg = row.HeadingDeg.HasValue ? (short?)Convert.ToInt16(Math.Round(row.HeadingDeg.Value)) : null,
        Hdop = row.Hdop.HasValue ? (decimal?)row.Hdop.Value : null,
        Satellites = row.Satellites.HasValue ? (short?)row.Satellites.Value : null,
        Ignition = row.Ignition,
    };

    private static VehicleLiveDto BuildLiveDto(PositionProcessOutcome outcome) => new(
        outcome.VehicleId,
        outcome.VehicleCode ?? string.Empty,
        outcome.ActiveTripId,
        outcome.Latitude,
        outcome.Longitude,
        outcome.SpeedKmh ?? 0,
        outcome.HeadingDeg ?? 0,
        DistanceKm: 0,
        Progress: 0,
        EtaSec: 0,
        LoadTonnes: null,
        MaterialName: null,
        OriginName: null,
        DestinationName: null,
        LastUpdated: outcome.RecordedAt);
}