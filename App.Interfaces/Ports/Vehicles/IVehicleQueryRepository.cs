using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Common.Query;

namespace App.Interfaces.Ports.Vehicles;

public interface IVehicleQueryRepository
{
    Task<QueryResult<VehicleStatusResponse>> GetVehiclesPagedAsync(
        int page,
        int pageSize,
        QueryFilter<VehicleStatusResponse>? filter = null,
        CancellationToken cancellationToken = default);
}