using App.Domain.Routes.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Roads;
using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Utils.Geometry;
using App.Shared.Common.Query;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.Roads;

public class RoadQueryRepository : BaseRepository<TMineRoad>, IRoadQueryRepository
{
    public RoadQueryRepository(AppDataBaseContext dbc) : base(dbc)
    {
    }

    public async Task<QueryResult<RoadResponse>> GetRoadsPagedAsync(
        int page,
        int pageSize,
        QueryFilter<RoadResponse>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var all = await _dbc.MineRoads
            .AsNoTracking()
            .Where(r => r.Active)
            .OrderBy(r => r.Code)
            .ToListAsync(cancellationToken);

        var mapped = all
            .Select(r => new RoadResponse(
                r.Id,
                r.Code,
                r.Name,
                GeoJsonConverter.ToGeoJson(r.Geometry),
                r.MaxSpeedKmh,
                r.Active,
                GeometryHelper.LengthKm(r.Geometry)))
            .ToList();

        IQueryable<RoadResponse> query = mapped.AsQueryable();
        if (filter is not null)
        {
            query = filter.ApplyFilter(query);
        }

        return PaginateInMemory(query.ToList(), page, pageSize);
    }
}