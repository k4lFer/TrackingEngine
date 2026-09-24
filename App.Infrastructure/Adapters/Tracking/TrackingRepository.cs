using App.Domain.Tracking.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Geometry;
using App.Shared.Objects.Enums;
using App.Shared.Query;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace App.Infrastructure.Adapters.Tracking;

/// <summary>
/// Lecturas y escrituras del motor de tracking. Las consultas de lectura cruzan
/// otros módulos (vehículos, geocercas, rutas, materiales) en una sola query con
/// JOINs que devuelve directamente el DTO de respuesta.
/// </summary>
public class TrackingRepository : BaseRepository<TTrip>, ITrackingReadRepository, ITrackingWriteRepository
{
    public TrackingRepository(AppDataBaseContext dbc) : base(dbc)
    {
    }

    public async Task<PositionResponse?> GetLatestPositionAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var p = await _dbc.GpsPositions.AsNoTracking()
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return p is null ? null : MapPosition(p);
    }

    public async Task<List<PositionResponse>> GetLatestPositionByVehicleAsync(
        CancellationToken cancellationToken = default)
    {
        var list = await _dbc.GpsPositions.AsNoTracking()
            .GroupBy(p => p.VehicleId)
            .Select(g => g.OrderByDescending(x => x.RecordedAt).First())
            .OrderBy(x => x.VehicleId)
            .ToListAsync(cancellationToken);

        return list.Select(MapPosition).ToList();
    }

    public async Task<List<PositionResponse>> GetPositionHistoryAsync(
        Guid vehicleId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var list = await _dbc.GpsPositions.AsNoTracking()
            .Where(p => p.VehicleId == vehicleId && p.RecordedAt >= fromUtc && p.RecordedAt <= toUtc)
            .OrderBy(p => p.RecordedAt)
            .ToListAsync(cancellationToken);

        return list.Select(MapPosition).ToList();
    }

    public async Task<List<TrackingEventResponse>> GetRecentEventsAsync(
        Guid vehicleId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            from e in _dbc.TrackingEvents.AsNoTracking()
            join v in _dbc.Vehicles.AsNoTracking() on e.VehicleId equals v.Id
            join g in _dbc.Geofences.AsNoTracking() on e.GeofenceId equals g.Id into gc
            from g in gc.DefaultIfEmpty()
            where e.VehicleId == vehicleId && e.OccurredAt >= fromUtc && e.OccurredAt <= toUtc
            orderby e.OccurredAt descending
            select new EventRow(e.Id, e.VehicleId, v.Code, e.TripId, e.Type, e.Severity,
                e.OccurredAt, g != null ? g.Name : null, e.PayloadJson))
            .ToListAsync(cancellationToken);

        return rows.Select(r => r.ToResponse()).ToList();
    }

    public async Task<RecentEventsResponse> GetRecentEventsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            from e in _dbc.TrackingEvents.AsNoTracking()
            join v in _dbc.Vehicles.AsNoTracking() on e.VehicleId equals v.Id
            join g in _dbc.Geofences.AsNoTracking() on e.GeofenceId equals g.Id into gc
            from g in gc.DefaultIfEmpty()
            orderby e.OccurredAt descending
            select new EventRow(e.Id, e.VehicleId, v.Code, e.TripId, e.Type, e.Severity,
                e.OccurredAt, g != null ? g.Name : null, e.PayloadJson))
            .Take(count)
            .ToListAsync(cancellationToken);

        var events = rows.Select(r => r.ToResponse()).ToList();
        return new RecentEventsResponse(events, events.Count);
    }

    public async Task<List<TrackingEventResponse>> GetEventsByTripAsync(
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            from e in _dbc.TrackingEvents.AsNoTracking()
            join v in _dbc.Vehicles.AsNoTracking() on e.VehicleId equals v.Id
            join g in _dbc.Geofences.AsNoTracking() on e.GeofenceId equals g.Id into gc
            from g in gc.DefaultIfEmpty()
            where e.TripId == tripId
            orderby e.OccurredAt
            select new EventRow(e.Id, e.VehicleId, v.Code, e.TripId, e.Type, e.Severity,
                e.OccurredAt, g != null ? g.Name : null, e.PayloadJson))
            .ToListAsync(cancellationToken);

        return rows.Select(r => r.ToResponse()).ToList();
    }

    public async Task<TripDetailResponse?> GetLatestActiveTripAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var row = await Trips(
                t => t.VehicleId == vehicleId
                     && t.Status == TripStatus.Active
                     && t.EndedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

        return row?.ToDetail();
    }

    public async Task<TripDetailResponse?> GetTripByIdAsync(
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        var row = await Trips(t => t.Id == tripId)
            .FirstOrDefaultAsync(cancellationToken);

        return row?.ToDetail();
    }

    public async Task<List<TripSummaryResponse>> GetTripsAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var list = await Trips(t => t.VehicleId == vehicleId)
            .ToListAsync(cancellationToken);

        return list.Select(t => t.ToSummary()).ToList();
    }

    public async Task<List<TripSummaryResponse>> GetAllTripsAsync(
        CancellationToken cancellationToken = default)
    {
        var list = await Trips().ToListAsync(cancellationToken);

        return list.Select(t => t.ToSummary()).ToList();
    }

    public async Task<QueryResult<TripSummaryResponse>> GetTripsPagedAsync(
        int page,
        int pageSize,
        QueryFilter<TripSummaryResponse>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var all = (await Trips().ToListAsync(cancellationToken))
            .Select(t => t.ToSummary())
            .ToList();

        IQueryable<TripSummaryResponse> query = all.AsQueryable();
        if (filter is not null)
        {
            query = filter.ApplyFilter(query);
        }

        return PaginateInMemory(query.ToList(), page, pageSize);
    }

    public async Task<TTrip?> GetActiveTripByVehicleAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbc.Trips
            .FirstOrDefaultAsync(
                t => t.VehicleId == vehicleId && t.Status == TripStatus.Active && t.EndedAt == null,
                cancellationToken);
    }

    public async Task<TTrip?> GetTripEntityByIdAsync(
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        return await _dbc.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId, cancellationToken);
    }

    public async Task<List<TTrip>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbc.Trips
            .Where(t => t.Status == TripStatus.Active && t.EndedAt == null)
            .ToListAsync(cancellationToken);
    }

    public void Add(TTrip trip) => _dbc.Trips.Add(trip);

    public void Add(TGpsPosition position) => _dbc.GpsPositions.Add(position);

    public void Add(TTrackingEvent trackingEvent) => _dbc.TrackingEvents.Add(trackingEvent);

    public void Update(TTrip trip) => _dbc.Trips.Update(trip);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbc.SaveChangesAsync(cancellationToken);

    private IQueryable<TripRow> Trips(Expression<Func<TTrip, bool>>? filter = null)
    {
        var src = filter == null
            ? _dbc.Trips.AsNoTracking()
            : _dbc.Trips.AsNoTracking().Where(filter);

        return
            from t in src
            join v in _dbc.Vehicles.AsNoTracking() on t.VehicleId equals v.Id
            join og in _dbc.Geofences.AsNoTracking() on t.OriginGeofenceId equals og.Id into ogc
            from og in ogc.DefaultIfEmpty()
            join dg in _dbc.Geofences.AsNoTracking() on t.DestinationGeofenceId equals dg.Id into dgc
            from dg in dgc.DefaultIfEmpty()
            join r in _dbc.Routes.AsNoTracking() on t.RouteId equals r.Id into rc
            from r in rc.DefaultIfEmpty()
            join m in _dbc.Materials.AsNoTracking() on t.MaterialId equals m.Id into mc
            from m in mc.DefaultIfEmpty()
            orderby t.StartedAt descending
            select new TripRow(
                t.Id,
                t.VehicleId,
                v.Code,
                t.RouteId,
                t.OriginGeofenceId,
                t.DestinationGeofenceId,
                og != null ? og.Name : null,
                dg != null ? dg.Name : null,
                r != null ? r.Code : null,
                r != null ? r.Name : null,
                m != null ? m.Name : null,
                t.LoadTonnes,
                t.StartedAt,
                t.EndedAt,
                t.DistanceKm,
                t.DurationS,
                t.MaxSpeedKmh,
                t.DeviationCount,
                t.SpeedAlertCount,
                t.Status,
                t.Track);
    }

    private static PositionResponse MapPosition(TGpsPosition p)
        => new(p.Id, p.VehicleId, p.RecordedAt, p.Geometry.Y, p.Geometry.X, p.SpeedKmh, true, null);

    private sealed record EventRow(
        Guid Id,
        Guid VehicleId,
        string VehicleCode,
        Guid? TripId,
        TrackingEventType Type,
        EventSeverity Severity,
        DateTime OccurredAt,
        string? GeofenceName,
        string? PayloadJson)
    {
        public TrackingEventResponse ToResponse() => new(
            Id,
            VehicleId,
            VehicleCode,
            TripId,
            Type.ToString(),
            Severity.ToString(),
            OccurredAt,
            GeofenceName,
            PayloadJson);
    }

    private sealed record TripRow(
        Guid Id,
        Guid VehicleId,
        string VehicleCode,
        Guid? RouteId,
        Guid? OriginGeofenceId,
        Guid? DestinationGeofenceId,
        string? OriginName,
        string? DestinationName,
        string? RouteCode,
        string? RouteName,
        string? MaterialName,
        decimal? LoadTonnes,
        DateTime StartedAt,
        DateTime? EndedAt,
        decimal? DistanceKm,
        int? DurationS,
        decimal? MaxSpeedKmh,
        int DeviationCount,
        int SpeedAlertCount,
        TripStatus Status,
        LineString? Track)
    {
        public TripSummaryResponse ToSummary() => new(
            Id,
            VehicleId,
            VehicleCode,
            RouteId,
            OriginGeofenceId,
            DestinationGeofenceId,
            OriginName,
            DestinationName,
            RouteCode,
            MaterialName,
            LoadTonnes,
            StartedAt,
            EndedAt,
            DistanceKm,
            DurationS,
            Status.ToString());

        public TripDetailResponse ToDetail() => new(
            Id,
            VehicleId,
            VehicleCode,
            RouteId,
            OriginGeofenceId,
            DestinationGeofenceId,
            OriginName,
            DestinationName,
            RouteName,
            MaterialName,
            LoadTonnes,
            StartedAt,
            EndedAt,
            DistanceKm,
            DurationS,
            MaxSpeedKmh,
            DeviationCount,
            SpeedAlertCount,
            Status.ToString(),
            Track is null ? null : GeoJsonConverter.ToGeoJson(Track));
    }
}