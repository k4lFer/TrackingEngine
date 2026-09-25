using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Vehicles.Query.GetById;

public class GetByIdVehicleQuery : IQuery<OutputPort<VehicleStatusResponse>>
{
    public Guid Id { get; }
    public GetByIdVehicleQuery(Guid id) => Id = id;
}