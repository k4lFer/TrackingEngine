using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetLatestPositions;

public class GetLatestPositionsQueryHandler : IQueryHandler<GetLatestPositionsQuery, OutputPort<List<PositionResponse>>>
{
    private readonly ITrackingReadRepository _read;

    public GetLatestPositionsQueryHandler(ITrackingReadRepository read)
    {
        _read = read;
    }

    public async Task<OutputPort<List<PositionResponse>>> Handle(GetLatestPositionsQuery query, CancellationToken cancellationToken)
    {
        var positions = await _read.GetLatestPositionByVehicleAsync(cancellationToken);
        return OutputPort<List<PositionResponse>>.Success(data: positions);
    }
}