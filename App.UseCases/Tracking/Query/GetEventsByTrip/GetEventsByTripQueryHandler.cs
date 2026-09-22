using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetEventsByTrip;

public class GetEventsByTripQueryHandler : IQueryHandler<GetEventsByTripQuery, OutputPort<List<TrackingEventResponse>>>
{
    private readonly ITrackingReadRepository _read;

    public GetEventsByTripQueryHandler(ITrackingReadRepository read)
    {
        _read = read;
    }

    public async Task<OutputPort<List<TrackingEventResponse>>> Handle(GetEventsByTripQuery query, CancellationToken cancellationToken)
    {
        var events = await _read.GetEventsByTripAsync(query.TripId, cancellationToken);
        return OutputPort<List<TrackingEventResponse>>.Success(data: events);
    }
}