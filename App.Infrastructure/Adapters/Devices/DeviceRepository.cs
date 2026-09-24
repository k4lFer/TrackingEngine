using App.Domain.Vehicles.Entities;
using App.Infrastructure.Adapters;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Devices;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.Devices;

public class DeviceRepository : BaseRepository<TDevice>, IDeviceRepository
{
    public DeviceRepository(AppDataBaseContext dbc) : base(dbc) { }

    public async Task<TDevice?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        return await _dbc.Devices
            .FirstOrDefaultAsync(d => d.Identifier == identifier, cancellationToken);
    }

    public async Task<TDevice?> GetBoundToVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _dbc.Devices
            .FirstOrDefaultAsync(d => d.VehicleId == vehicleId, cancellationToken);
    }
}