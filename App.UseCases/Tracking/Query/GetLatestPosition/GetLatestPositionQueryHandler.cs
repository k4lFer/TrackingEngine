using System.Net;
using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetLatestPosition;

public class GetLatestPositionQueryHandler : IQueryHandler<GetLatestPositionQuery, OutputPort<PositionResponse?>>
{
    private readonly ITrackingReadRepository _read;

    public GetLatestPositionQueryHandler(ITrackingReadRepository read)
    {
        _read = read;
    }

    public async Task<OutputPort<PositionResponse?>> Handle(GetLatestPositionQuery query, CancellationToken cancellationToken)
    {
        var position = await _read.GetLatestPositionAsync(query.VehicleId, cancellationToken);
        if (position is null)
        {
            return OutputPort<PositionResponse?>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró la última posición del vehículo.", "POSITION_NOT_FOUND"));
        }

        return OutputPort<PositionResponse?>.Success(data: position);
    }
}