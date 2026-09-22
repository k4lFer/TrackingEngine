using System.Net;
using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetTripTrack;

public class GetTripTrackQueryHandler : IQueryHandler<GetTripTrackQuery, OutputPort<List<PositionResponse>>>
{
    private readonly ITrackingReadRepository _read;

    public GetTripTrackQueryHandler(ITrackingReadRepository read)
    {
        _read = read;
    }

    public async Task<OutputPort<List<PositionResponse>>> Handle(GetTripTrackQuery query, CancellationToken cancellationToken)
    {
        var trip = await _read.GetTripByIdAsync(query.TripId, cancellationToken);
        if (trip is null)
        {
            return OutputPort<List<PositionResponse>>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró el viaje indicado.", "TRIP_NOT_FOUND"));
        }

        var positions = await _read.GetPositionHistoryAsync(
            trip.VehicleId,
            trip.StartedAt,
            trip.EndedAt ?? DateTime.UtcNow,
            cancellationToken);

        return OutputPort<List<PositionResponse>>.Success(data: positions);
    }
}