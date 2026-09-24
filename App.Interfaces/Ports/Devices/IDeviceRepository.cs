using App.Domain.Vehicles.Entities;

namespace App.Interfaces.Ports.Devices;

public interface IDeviceRepository
{
    Task<TDevice?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);

    /// <summary>Devuelve el dispositivo vinculado a un vehículo (si tiene).</summary>
    Task<TDevice?> GetBoundToVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    Task AddAsync(TDevice device, CancellationToken cancellationToken = default);
}