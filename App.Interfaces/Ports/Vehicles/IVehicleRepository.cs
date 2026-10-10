using App.Domain.Vehicles.Entities;
using App.Objects.Vehicles.DTOs.Output.Response;

namespace App.Interfaces.Ports.Vehicles;

public interface IVehicleRepository : IBaseRepository<TVehicle>
{
    Task<TVehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TVehicle?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<TVehicle?> GetByDeviceIdentifierAsync(string identifier, CancellationToken cancellationToken = default);
    Task<VehicleIngestSnapshot?> GetIngestSnapshotByDeviceIdentifierAsync(string identifier, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(string Identifier, Guid VehicleId)>> GetAllDeviceVehiclePairsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VehicleStatusResponse>> GetAllWithStatusAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VehicleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task RemoveAsync(TVehicle vehicle, CancellationToken cancellationToken = default);

    Task TouchLastReceivedAsync(Guid vehicleId, DateTime receivedAt, CancellationToken cancellationToken = default);
}