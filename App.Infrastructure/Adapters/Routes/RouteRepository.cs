using App.Domain.Routes.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Routes;
using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Geometry;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.Routes;

public class RouteRepository : IRouteRepository, IRouteGeometryRepository
{
    private const int DefaultMaxSpeedKmh = 40;

    private readonly AppDataBaseContext _dbc;

    public RouteRepository(AppDataBaseContext dbc)
    {
        _dbc = dbc;
    }

    public async Task<IReadOnlyList<RouteResponse>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var rows = await (
            from r in _dbc.Routes.AsNoTracking()
            join og in _dbc.Geofences.AsNoTracking() on r.OriginGeofenceId equals og.Id into ogJoined
            from og in ogJoined.DefaultIfEmpty()
            join dg in _dbc.Geofences.AsNoTracking() on r.DestinationGeofenceId equals dg.Id into dgJoined
            from dg in dgJoined.DefaultIfEmpty()
            where r.Active && r.AlternativeRank == 0
            orderby r.Code
            select new RouteRow(
                r.Id,
                r.Code,
                r.Name,
                r.Geometry,
                r.ToleranceM,
                r.MaxSpeedKmh,
                r.Active,
                r.OriginGeofenceId,
                og != null ? og.Name : null,
                r.DestinationGeofenceId,
                dg != null ? dg.Name : null,
                r.RouteGroupId,
                r.AlternativeRank))
            .ToListAsync(cancellationToken);

        var groupIds = rows
            .Where(r => r.RouteGroupId is not null)
            .Select(r => r.RouteGroupId!.Value)
            .ToHashSet();

        var alternativesByGroup = groupIds.Count == 0
            ? new Dictionary<Guid, List<RouteAlternativeResponse>>()
            : (await (
                from r in _dbc.Routes.AsNoTracking()
                where r.Active && r.AlternativeRank > 0 && r.RouteGroupId != null && groupIds.Contains(r.RouteGroupId.Value)
                orderby r.RouteGroupId, r.AlternativeRank
                select new RouteRow(
                    r.Id,
                    r.Code,
                    r.Name,
                    r.Geometry,
                    r.ToleranceM,
                    r.MaxSpeedKmh,
                    r.Active,
                    r.OriginGeofenceId,
                    null,
                    r.DestinationGeofenceId,
                    null,
                    r.RouteGroupId,
                    r.AlternativeRank))
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.RouteGroupId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(a => MapToAlternative(a, includeGeometry: false)).ToList());

        return rows.Select(r => MapToResponse(r, alternativesFor(r.RouteGroupId, alternativesByGroup))).ToList();
    }

    private static List<RouteAlternativeResponse>? alternativesFor(
        Guid? routeGroupId,
        IReadOnlyDictionary<Guid, List<RouteAlternativeResponse>> groups) =>
        routeGroupId is Guid gid && groups.TryGetValue(gid, out var list) ? list : null;

    public async Task<RouteResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await (
            from r in _dbc.Routes.AsNoTracking()
            join og in _dbc.Geofences.AsNoTracking() on r.OriginGeofenceId equals og.Id into ogJoined
            from og in ogJoined.DefaultIfEmpty()
            join dg in _dbc.Geofences.AsNoTracking() on r.DestinationGeofenceId equals dg.Id into dgJoined
            from dg in dgJoined.DefaultIfEmpty()
            where r.Id == id
            select new RouteRow(
                r.Id,
                r.Code,
                r.Name,
                r.Geometry,
                r.ToleranceM,
                r.MaxSpeedKmh,
                r.Active,
                r.OriginGeofenceId,
                og != null ? og.Name : null,
                r.DestinationGeofenceId,
                dg != null ? dg.Name : null,
                r.RouteGroupId,
                r.AlternativeRank))
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return null;

        var alternatives = new List<RouteAlternativeResponse>();
        if (row.RouteGroupId is Guid groupId)
        {
            var altRows = await (
                from r in _dbc.Routes.AsNoTracking()
                where r.RouteGroupId == groupId && r.AlternativeRank > 0
                orderby r.AlternativeRank
                select new RouteRow(
                    r.Id,
                    r.Code,
                    r.Name,
                    r.Geometry,
                    r.ToleranceM,
                    r.MaxSpeedKmh,
                    r.Active,
                    r.OriginGeofenceId,
                    null,
                    r.DestinationGeofenceId,
                    null,
                    r.RouteGroupId,
                    r.AlternativeRank))
                .ToListAsync(cancellationToken);

            alternatives = altRows.Select(a => MapToAlternative(a, includeGeometry: true)).ToList();
        }

        return MapToResponse(row, alternatives);
    }

    public async Task<TRoute?> GetByIdRawAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbc.Routes
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbc.Routes
            .AnyAsync(r => r.Code == code, cancellationToken);
    }

    public async Task<List<TMineRoad>> GetAllActiveRoadsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbc.MineRoads
            .AsNoTracking()
            .Where(r => r.Active)
            .ToListAsync(cancellationToken);
    }

    public void Add(TRoute route)
    {
        _dbc.Routes.Add(route);
    }

    public void Remove(TRoute route)
    {
        _dbc.Routes.Remove(route);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbc.SaveChangesAsync(cancellationToken);
    }

    private static RouteResponse MapToResponse(RouteRow row, IReadOnlyList<RouteAlternativeResponse>? alternatives = null)
    {
        var geoJson = GeoJsonConverter.ToGeoJson(row.Geometry);
        var km = GeometryHelper.LengthKm(row.Geometry);

        return new RouteResponse(
            row.Id,
            row.Code,
            row.Name,
            geoJson,
            row.ToleranceM,
            row.MaxSpeedKmh,
            row.Active,
            km,
            EstimateDurationS(row.MaxSpeedKmh, km),
            row.OriginGeofenceId,
            row.OriginGeofenceName,
            row.DestinationGeofenceId,
            row.DestinationGeofenceName,
            alternatives);
    }

    private static RouteAlternativeResponse MapToAlternative(RouteRow row, bool includeGeometry)
    {
        var geoJson = includeGeometry ? GeoJsonConverter.ToGeoJson(row.Geometry) : null;
        var km = GeometryHelper.LengthKm(row.Geometry);

        return new RouteAlternativeResponse(
            row.Id,
            row.Code,
            row.Name,
            row.AlternativeRank,
            geoJson,
            km,
            EstimateDurationS(row.MaxSpeedKmh, km));
    }

    private static double EstimateDurationS(int? maxSpeedKmh, double km)
    {
        if (km <= 0) return 0;
        double speed = maxSpeedKmh is > 0 ? maxSpeedKmh.Value : DefaultMaxSpeedKmh;
        return km / speed * 3600;
    }

    private sealed record RouteRow(
        Guid Id,
        string Code,
        string Name,
        NetTopologySuite.Geometries.LineString Geometry,
        int ToleranceM,
        int? MaxSpeedKmh,
        bool Active,
        Guid? OriginGeofenceId,
        string? OriginGeofenceName,
        Guid? DestinationGeofenceId,
        string? DestinationGeofenceName,
        Guid? RouteGroupId,
        int AlternativeRank);
}