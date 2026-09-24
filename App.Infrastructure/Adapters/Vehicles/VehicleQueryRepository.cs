using App.Domain.Vehicles.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.Vehicles;

public class VehicleQueryRepository : BaseRepository<TVehicle>, IVehicleQueryRepository
{
    public VehicleQueryRepository(AppDataBaseContext dbc) : base(dbc)
    {
    }

    public async Task<QueryResult<VehicleStatusResponse>> GetVehiclesPagedAsync(
        int page,
        int pageSize,
        QueryFilter<VehicleStatusResponse>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var all = await _dbc.Vehicles
            .AsNoTracking()
            .OrderBy(v => v.Id)
            .Select(v => new VehicleStatusResponse(
                v.Id,
                v.Code,
                v.Plate,
                v.Brand,
                v.Model,
                v.Active,
                _dbc.Devices.Where(d => d.VehicleId == v.Id).Select(d => d.Identifier).FirstOrDefault(),
                _dbc.Devices.Where(d => d.VehicleId == v.Id).Select(d => (int?)d.Kind).FirstOrDefault(),
                _dbc.Devices.Where(d => d.VehicleId == v.Id).Select(d => d.Model).FirstOrDefault(),
                v.CurrentState != null ? v.CurrentState.State.ToString() : "Offline",
                v.CurrentState != null && v.CurrentState.LastGeom != null ? v.CurrentState.LastGeom.Y : null,
                v.CurrentState != null && v.CurrentState.LastGeom != null ? v.CurrentState.LastGeom.X : null,
                v.CurrentState != null ? v.CurrentState.LastReceivedAt : v.LastReportedAt,
                v.CurrentState != null && v.CurrentState.ActiveTripId != null ? v.CurrentState.ActiveTripId : null))
            .ToListAsync(cancellationToken);

        IQueryable<VehicleStatusResponse> query = all.AsQueryable();
        if (filter is not null)
        {
            query = filter.ApplyFilter(query);
        }

        return PaginateInMemory(query.ToList(), page, pageSize);
    }
}