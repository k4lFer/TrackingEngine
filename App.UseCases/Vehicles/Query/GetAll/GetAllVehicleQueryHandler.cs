using App.Interfaces.Ports.Vehicles;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Common.Query;
using App.Shared.Common.Result;
using App.UseCases.Vehicles.Query.Filter;
using Cortex.Mediator.Queries;

namespace App.UseCases.Vehicles.Query.GetAll;

public class GetAllVehicleQueryHandler : IQueryHandler<GetAllVehicleQuery, OutputPort<QueryResult<VehicleStatusResponse>>>
{
    private readonly IVehicleQueryRepository _vehicleQueryRepository;

    public GetAllVehicleQueryHandler(IVehicleQueryRepository vehicleQueryRepository)
    {
        _vehicleQueryRepository = vehicleQueryRepository;
    }

    public async Task<OutputPort<QueryResult<VehicleStatusResponse>>> Handle(GetAllVehicleQuery query, CancellationToken cancellationToken)
    {
        var filter = new FilterAllVehicles
        {
            Search = query.Filter.Search,
            State = query.Filter.State,
            OnlyOnline = query.Filter.OnlyOnline,
        };

        var results = await _vehicleQueryRepository.GetVehiclesPagedAsync(
            query.Filter.NumberPage,
            query.Filter.PageSize,
            filter,
            cancellationToken);

        return OutputPort<QueryResult<VehicleStatusResponse>>.Success(data: results);
    }
}