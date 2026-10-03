using App.Domain.Vehicles.Entities;
using App.Objects.Vehicles.DTOs.Output.Response;

namespace App.Interfaces.Ports.Vehicles;

public interface IVehicleRepository : IBaseRepository<TVehicle>
{
    Task<TVehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TVehicle?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<TVehicle?> GetByDeviceIdentifierAsync(string identifier, CancellationToken cancellationToken = default);

    /// <summary>Vehiculos con CurrentState cargado para el estado en vivo.</summary>
    Task<IReadOnlyList<VehicleStatusResponse>> GetAllWithStatusAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VehicleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task RemoveAsync(TVehicle vehicle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresca "última señal" sin pasar por el pipeline de ReportPosition. Se usa
    /// cuando la trama fue suprimida por el filtro pero el vehículo sí está
    /// transmitiendo: sin esto, "última señal" se congelaría en la hora del último
    /// punto guardado y el vehículo parecería caído.
    /// </summary>
    Task TouchLastReceivedAsync(Guid vehicleId, DateTime receivedAt, CancellationToken cancellationToken = default);
}