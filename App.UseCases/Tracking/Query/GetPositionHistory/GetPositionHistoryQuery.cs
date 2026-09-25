using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetPositionHistory;

public class GetPositionHistoryQuery : IQuery<OutputPort<List<PositionResponse>>>
{
    public Guid VehicleId { get; }
    public DateTime From { get; }
    public DateTime To { get; }

    public GetPositionHistoryQuery(Guid vehicleId, DateTime from, DateTime to)
    {
        VehicleId = vehicleId;
        From = from;
        To = to;
    }
}