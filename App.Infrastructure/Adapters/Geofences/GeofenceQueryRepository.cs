using App.Domain.Geofences.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Geofences;
using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Geometry;
using App.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.Geofences;

public class GeofenceQueryRepository : BaseRepository<TGeofence>, IGeofenceQueryRepository
{
    public GeofenceQueryRepository(AppDataBaseContext dbc) : base(dbc)
    {
    }

    public async Task<QueryResult<GeofenceResponse>> GetGeofencesPagedAsync(
        int page,
        int pageSize,
        QueryFilter<GeofenceResponse>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var all = await _dbc.Geofences
            .AsNoTracking()
            .OrderBy(g => g.Code)
            .ToListAsync(cancellationToken);

        var mapped = all
            .Select(g => new GeofenceResponse(
                g.Id,
                g.Code,
                g.Name,
                g.Kind.ToString(),
                g.Priority,
                GeoJsonConverter.ToGeoJson(g.Geometry),
                g.MaxSpeedKmh,
                g.Color,
                g.Active))
            .ToList();

        IQueryable<GeofenceResponse> query = mapped.AsQueryable();
        if (filter is not null)
        {
            query = filter.ApplyFilter(query);
        }

        return PaginateInMemory(query.ToList(), page, pageSize);
    }
}