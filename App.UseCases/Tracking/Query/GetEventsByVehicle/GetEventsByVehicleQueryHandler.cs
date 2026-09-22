using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetEventsByVehicle;

public class GetEventsByVehicleQueryHandler : IQueryHandler<GetEventsByVehicleQuery, OutputPort<List<TrackingEventResponse>>>
{
    private readonly ITrackingReadRepository _read;

    public GetEventsByVehicleQueryHandler(ITrackingReadRepository read)
    {
        _read = read;
    }

    public async Task<OutputPort<List<TrackingEventResponse>>> Handle(GetEventsByVehicleQuery query, CancellationToken cancellationToken)
    {
        var events = await _read.GetRecentEventsAsync(query.VehicleId, query.From, query.To, cancellationToken);
        return OutputPort<List<TrackingEventResponse>>.Success(data: events);
    }
}