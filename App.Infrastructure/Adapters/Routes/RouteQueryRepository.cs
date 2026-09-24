using System.Text.Json;
using App.Domain.Routes.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Routes;
using App.Objects.Routes.DTOs.Output.Response;
using App.Objects.Shared.DTOs;
using App.Shared.Geometry;
using App.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.Routes;

public class RouteQueryRepository : BaseRepository<TRoute>, IRouteQueryRepository
{
    private const int DefaultMaxSpeedKmh = 40;

    public RouteQueryRepository(AppDataBaseContext dbc) : base(dbc)
    {
    }

    public async Task<QueryResult<RouteResponse>> GetRoutesPagedAsync(
        int page,
        int pageSize,
        QueryFilter<RouteResponse>? filter = null,
        bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var mainRows = await (
            from r in _dbc.Routes.AsNoTracking()
            join og in _dbc.Geofences.AsNoTracking() on r.OriginGeofenceId equals og.Id into ogJoined
            from og in ogJoined.DefaultIfEmpty()
            join dg in _dbc.Geofences.AsNoTracking() on r.DestinationGeofenceId equals dg.Id into dgJoined
            from dg in dgJoined.DefaultIfEmpty()
            where r.AlternativeRank == 0 && r.Active == onlyActive
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
                r.AlternativeRank,
                r.WaypointsJson))
            .ToListAsync(cancellationToken);

        var groupIds = mainRows
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
                    r.AlternativeRank,
                    null))
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.RouteGroupId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(a => MapToAlternative(a, includeGeometry: false)).ToList());

        var mapped = mainRows
            .Select(r => MapToResponse(r, alternativesFor(r.RouteGroupId, alternativesByGroup)))
            .ToList();

        IQueryable<RouteResponse> query = mapped.AsQueryable();
        if (filter is not null)
        {
            query = filter.ApplyFilter(query);
        }

        return PaginateInMemory(query.ToList(), page, pageSize);
    }

    private static List<RouteAlternativeResponse>? alternativesFor(
        Guid? routeGroupId,
        IReadOnlyDictionary<Guid, List<RouteAlternativeResponse>> groups) =>
        routeGroupId is Guid gid && groups.TryGetValue(gid, out var list) ? list : null;

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
            alternatives,
            Waypoints: ParseWaypoints(row.WaypointsJson));
    }

    private static IReadOnlyList<CoordinateDto>? ParseWaypoints(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<List<CoordinateDto>>(json);
        }
        catch (JsonException)
        {
            return null;
        }
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
        int AlternativeRank,
        string? WaypointsJson);
}