using App.Objects.Vehicles.DTOs.Input.Query;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Common.Query;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Vehicles.Query.GetAll;

public class GetAllVehicleQuery : IQuery<OutputPort<QueryResult<VehicleStatusResponse>>>
{
    public VehicleFilterDto Filter { get; }

    public GetAllVehicleQuery(VehicleFilterDto filter)
    {
        Filter = filter;
    }
}