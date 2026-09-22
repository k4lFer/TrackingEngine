using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetEventsByTrip;

public class GetEventsByTripQuery : IQuery<OutputPort<List<TrackingEventResponse>>>
{
    public Guid TripId { get; }

    public GetEventsByTripQuery(Guid tripId) => TripId = tripId;
}