using App.Domain.Vehicles.Entities;
using App.Infrastructure.Adapters;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Vehicles.DTOs.Output.Response;
using Microsoft.EntityFrameworkCore;
using App.Infrastructure.Core.DataBaseContext.Connection;

namespace App.Infrastructure.Adapters.Vehicles;

public class VehicleRepository : BaseRepository<TVehicle>, IVehicleRepository
{
    public VehicleRepository(AppDataBaseContext dbc) : base(dbc) { }

    public async Task<TVehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbc.Vehicles
            .Include(v => v.CurrentState)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<TVehicle?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbc.Vehicles
            .Include(v => v.CurrentState)
            .FirstOrDefaultAsync(v => v.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<VehicleStatusResponse>> GetAllWithStatusAsync(CancellationToken cancellationToken = default)
    {
        return await _dbc.Vehicles
            .AsNoTracking()
            .Select(v => new VehicleStatusResponse(
                v.Id,
                v.Code,
                v.Plate,
                v.CurrentState != null ? v.CurrentState.State.ToString() : "Offline",
                v.CurrentState != null && v.CurrentState.LastGeom != null ? v.CurrentState.LastGeom.Y : null,
                v.CurrentState != null && v.CurrentState.LastGeom != null ? v.CurrentState.LastGeom.X : null,
                v.CurrentState != null ? v.CurrentState.LastReceivedAt : v.LastReportedAt,
                v.CurrentState != null && v.CurrentState.ActiveTripId != null ? v.CurrentState.ActiveTripId : null))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VehicleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbc.Vehicles
            .AsNoTracking()
            .OrderBy(v => v.Id)
            .Select(v => new VehicleResponse(
                v.Id,
                v.Code,
                v.Plate,
                v.Brand,
                v.Model,
                v.Active))
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveAsync(TVehicle vehicle, CancellationToken cancellationToken = default)
    {
        _dbc.Vehicles.Remove(vehicle);
        await Task.CompletedTask;
    }
}
