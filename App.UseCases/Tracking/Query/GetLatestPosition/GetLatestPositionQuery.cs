using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetLatestPosition;

public class GetLatestPositionQuery : IQuery<OutputPort<PositionResponse?>>
{
    public Guid VehicleId { get; }

    public GetLatestPositionQuery(Guid vehicleId) => VehicleId = vehicleId;
}