using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Vehicles.Query.GetAll;

public class GetAllVehicleQuery : IQuery<OutputPort<QueryResult<VehicleStatusResponse>>>
{
}