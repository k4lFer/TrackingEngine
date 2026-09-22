using System.Net;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Vehicles.Query.GetAll;

public class GetAllVehicleQueryHandler : IQueryHandler<GetAllVehicleQuery, OutputPort<QueryResult<VehicleStatusResponse>>>
{
    private readonly IVehicleRepository _vehicleRepository;

    public GetAllVehicleQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<OutputPort<QueryResult<VehicleStatusResponse>>> Handle(GetAllVehicleQuery query, CancellationToken cancellationToken)
    {
        var results = await _vehicleRepository.GetAllWithStatusAsync(cancellationToken);
        return OutputPort<QueryResult<VehicleStatusResponse>>.Success(data: QueryResult<VehicleStatusResponse>.Success(results, results.Count, 1, 1, results.Count));
    }
}