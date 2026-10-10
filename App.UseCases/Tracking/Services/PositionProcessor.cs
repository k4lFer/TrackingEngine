using System.Text.Json;
using App.Domain.Geofences.Entities;
using App.Domain.Tracking.Entities;
using App.Domain.Vehicles.Entities;
using App.Interfaces.Ports.Geofences;
using App.Interfaces.Ports.Routes;
using App.Interfaces.Ports.Tracking;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Tracking.DTOs.Input.Command;
using App.Shared.Utils.Geometry;
using App.Shared.Common.Enums;
using App.Shared.Common.Gps;
using Microsoft.Extensions.Options;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace App.UseCases.Tracking.Services;

/// <inheritdoc />
public class PositionProcessor : IPositionProcessor
{
    private const short DeviceOffRouteThreshold = 2;

    private static readonly GeometryFactory _gf =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    private readonly ITrackingWriteRepository _trackingWrite;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IGeofenceRepository _geofenceRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IOptions<GpsRealtimeOptions> _gpsOptions;

    public PositionProcessor(
        ITrackingWriteRepository trackingWrite,
        IVehicleRepository vehicleRepository,
        IGeofenceRepository geofenceRepository,
        IRouteRepository routeRepository,
        IOptions<GpsRealtimeOptions> gpsOptions)
    {
        _trackingWrite = trackingWrite;
        _vehicleRepository = vehicleRepository;
        _geofenceRepository = geofenceRepository;
        _routeRepository = routeRepository;
        _gpsOptions = gpsOptions;
    }

    public async Task<PositionProcessOutcome> ProcessAsync(
        PositionReportRequest dto,
        CancellationToken cancellationToken)
    {
        if (!IsValidPosition(dto))
        {
            return Failed(dto, "Coordenadas inválidas");
        }

        var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            return Failed(dto, "Vehículo no encontrado");
        }

        var point = _gf.CreatePoint(new Coordinate(dto.Longitude, dto.Latitude));

        var persistedLast = vehicle.LastReportedAt;
        var isBackfill = persistedLast is not null
                         && persistedLast.Value <= DateTime.UtcNow.AddMinutes(5)
                         && dto.RecordedAt <= persistedLast.Value;

        var position = TGpsPosition.Create(
            vehicle.Id,
            dto.DeviceId,
            dto.RecordedAt,
            point,
            dto.SpeedKmh,
            dto.HeadingDeg,
            dto.Ignition,
            dto.OdometerKm,
            dto.Hdop,
            dto.Satellites);
        _trackingWrite.Add(position);

        if (isBackfill)
        {
            return new PositionProcessOutcome(
                true, null, position.Id, vehicle.Id, vehicle.Code, null, dto.RecordedAt,
                dto.Latitude, dto.Longitude, dto.SpeedKmh ?? 0, dto.HeadingDeg ?? 0,
                false, vehicle.CurrentState?.State ?? VehicleState.Offline, true);
        }

        vehicle.RecordLastPosition(point, dto.RecordedAt);

        var state = vehicle.CurrentState;
        var isNewState = state is null;
        if (isNewState)
        {
            state = TVehicleCurrentState.Create(vehicle.Id);
            vehicle.AttachState(state);
        }

        var activeTrip = await _trackingWrite.GetActiveTripByVehicleAsync(vehicle.Id, cancellationToken);
        state.SetActiveTrip(activeTrip?.Id);

        var geofence = await _geofenceRepository.GetFirstContainingAsync(point, cancellationToken);

        if (geofence is not null && state.CurrentGeofenceId != geofence.Id)
        {
            state.ApplyGeofence(geofence.Id);
            _trackingWrite.Add(TTrackingEvent.Create(
                vehicle.Id,
                activeTrip?.Id,
                TrackingEventType.GeofenceEntered,
                EventSeverity.Info,
                geofence.Id,
                point,
                $"{{\"geofence\":\"{geofence.Name}\"}}"));
        }
        else if (geofence is null && state.CurrentGeofenceId.HasValue)
        {
            _trackingWrite.Add(TTrackingEvent.Create(
                vehicle.Id,
                activeTrip?.Id,
                TrackingEventType.GeofenceExited,
                EventSeverity.Info,
                null,
                point,
                null));
            state.ApplyGeofence(null);
        }

        var speed = dto.SpeedKmh ?? 0;
        var previousState = state.State;
        var previousStoppedSince = state.StoppedSince;
        var previousLastReceivedAt = state.LastReceivedAt;
        var resolvedState = GpsStateResolver.Resolve(dto.SpeedKmh, geofence?.Kind, _gpsOptions.Value.StopSpeedKmh);

        var minStateDuration = TimeSpan.FromSeconds(Math.Max(0, _gpsOptions.Value.StateMinSeconds));
        var nowUtc = DateTime.UtcNow;
        var effectiveState = state.IsStateChangeSettled(resolvedState, nowUtc, minStateDuration)
            ? resolvedState
            : state.State;

        state.ReportPosition(point, effectiveState, dto.RecordedAt, nowUtc);
        var isStateTransition = previousState != state.State;

        HandleLongStopEvent(vehicle, activeTrip, geofence, point, state, previousState, previousStoppedSince, isStateTransition);
        HandleGpsConnectionRestored(vehicle, activeTrip, geofence, point, previousLastReceivedAt, previousState, isStateTransition, isNewState);

        var zoneLimit = geofence?.MaxSpeedKmh;
        if (zoneLimit is not null && speed > zoneLimit)
        {
            if (state.OverSpeedSince is null)
            {
                state.MarkOverspeed(dto.RecordedAt);
                _trackingWrite.Add(TTrackingEvent.Create(
                    vehicle.Id,
                    activeTrip?.Id,
                    TrackingEventType.SpeedLimitExceeded,
                    EventSeverity.Warning,
                    geofence!.Id,
                    point,
                    $"{{\"zone\":\"{geofence.Name}\",\"limit\":{(int)zoneLimit},\"speed\":{speed:0.0}}}"));
            }
        }
        else if (state.OverSpeedSince is not null)
        {
            state.ClearOverspeed();
            _trackingWrite.Add(TTrackingEvent.Create(
                vehicle.Id,
                activeTrip?.Id,
                TrackingEventType.SpeedLimitCleared,
                EventSeverity.Info,
                geofence?.Id,
                point,
                null));
        }

        await HandleRouteDeviationAsync(vehicle, activeTrip, point, dto, state, cancellationToken);

        return new PositionProcessOutcome(
            true, null, position.Id, vehicle.Id, vehicle.Code, activeTrip?.Id, dto.RecordedAt,
            dto.Latitude, dto.Longitude, speed, dto.HeadingDeg ?? 0,
            isStateTransition, state.State, false);
    }

    private void HandleLongStopEvent(
        TVehicle vehicle,
        TTrip? activeTrip,
        TGeofence? geofence,
        Point point,
        TVehicleCurrentState state,
        VehicleState previousState,
        DateTime? previousStoppedSince,
        bool isStateTransition)
    {
        if (isStateTransition && previousState == VehicleState.Detenido)
        {
            var seconds = previousStoppedSince is null
                ? 0
                : (int)(DateTime.UtcNow - previousStoppedSince.Value).TotalSeconds;

            _trackingWrite.Add(TTrackingEvent.Create(
                vehicle.Id, activeTrip?.Id, TrackingEventType.LongStopEnded,
                EventSeverity.Info, geofence?.Id, point, BuildLongStopPayload(geofence, seconds)));
            return;
        }

        if (state.State != VehicleState.Detenido || state.StoppedSince is null) return;
        if (state.LongStopNotifiedAt is not null) return;

        var stoppedSeconds = (int)(DateTime.UtcNow - state.StoppedSince.Value).TotalSeconds;
        if (stoppedSeconds < _gpsOptions.Value.LongStopThresholdSeconds) return;

        state.MarkLongStopNotified();
        _trackingWrite.Add(TTrackingEvent.Create(
            vehicle.Id, activeTrip?.Id, TrackingEventType.LongStopDetected,
            EventSeverity.Info, geofence?.Id, point, BuildLongStopPayload(geofence, stoppedSeconds)));
    }

    private void HandleGpsConnectionRestored(
        TVehicle vehicle,
        TTrip? activeTrip,
        TGeofence? geofence,
        Point point,
        DateTime? previousLastReceivedAt,
        VehicleState previousState,
        bool isStateTransition,
        bool isNewState)
    {
        if (isNewState) return;
        if (!isStateTransition || previousState != VehicleState.Offline) return;

        var payload = JsonSerializer.Serialize(new
        {
            last_received_at = previousLastReceivedAt,
            offline_for_s = previousLastReceivedAt is null
                ? (int?)null
                : (int)(DateTime.UtcNow - previousLastReceivedAt.Value).TotalSeconds,
            offline_after_s = (int)_gpsOptions.Value.OfflineThresholdSeconds,
        });

        _trackingWrite.Add(TTrackingEvent.Create(
            vehicle.Id, activeTrip?.Id, TrackingEventType.GpsConnectionRestored,
            EventSeverity.Info, geofence?.Id, point, payload));
    }

    private static string BuildLongStopPayload(TGeofence? geofence, int durationSeconds)
    {
        var name = geofence is null ? "null" : JsonSerializer.Serialize(geofence.Name);
        return $"{{\"geofence\":{name},\"duration_s\":{durationSeconds}}}";
    }

    private async Task HandleRouteDeviationAsync(
        TVehicle vehicle,
        TTrip? activeTrip,
        Point point,
        PositionReportRequest dto,
        TVehicleCurrentState state,
        CancellationToken cancellationToken)
    {
        if (activeTrip?.RouteId is not Guid routeId ||
            await _routeRepository.GetByIdRawAsync(routeId, cancellationToken) is not { } route)
        {
            state.ClearOffRoute();
            return;
        }

        var onRoute = GeometryHelper.DistanceM(route.Geometry, point) <= route.ToleranceM;

        if (onRoute)
        {
            if (state.OffRouteSince is not null)
            {
                state.ClearOffRoute();
                _trackingWrite.Add(TTrackingEvent.Create(
                    vehicle.Id,
                    activeTrip.Id,
                    TrackingEventType.RouteDeviationCleared,
                    EventSeverity.Info,
                    null,
                    point,
                    null));
            }
            else
            {
                state.ClearOffRoute();
            }
            return;
        }

        state.AddOffRouteStreak();
        if (state.OffRouteSince is not null || state.OffRouteStreak < DeviceOffRouteThreshold)
        {
            return;
        }

        state.MarkOffRoute(dto.RecordedAt);
        activeTrip.AddDeviation();

        var distanceM = GeometryHelper.DistanceM(route.Geometry, point);
        _trackingWrite.Add(TTrackingEvent.Create(
            vehicle.Id,
            activeTrip.Id,
            TrackingEventType.RouteDeviationDetected,
            EventSeverity.Warning,
            null,
            point,
            $"{{\"route_id\":{routeId},\"distance_m\":{distanceM:0.0}}}"));
    }

    private static bool IsValidPosition(PositionReportRequest r)
        => r.Latitude is >= -90 and <= 90
           && r.Longitude is >= -180 and <= 180
           && !(r.Latitude == 0 && r.Longitude == 0);

    private static PositionProcessOutcome Failed(PositionReportRequest dto, string reason)
        => new(false, reason, Guid.Empty, dto.VehicleId, null, null, dto.RecordedAt,
            dto.Latitude, dto.Longitude, dto.SpeedKmh ?? 0, dto.HeadingDeg ?? 0,
            false, VehicleState.Offline, false);
}
