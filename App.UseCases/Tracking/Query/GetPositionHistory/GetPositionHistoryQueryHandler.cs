using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetPositionHistory;

public class GetPositionHistoryQueryHandler : IQueryHandler<GetPositionHistoryQuery, OutputPort<List<PositionResponse>>>
{
    private readonly ITrackingReadRepository _read;

    public GetPositionHistoryQueryHandler(ITrackingReadRepository read)
    {
        _read = read;
    }

    public async Task<OutputPort<List<PositionResponse>>> Handle(GetPositionHistoryQuery query, CancellationToken cancellationToken)
    {
        var positions = await _read.GetPositionHistoryAsync(query.VehicleId, query.From, query.To, cancellationToken);
        return OutputPort<List<PositionResponse>>.Success(data: positions);
    }
}