using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetTripTrack;

public class GetTripTrackQuery : IQuery<OutputPort<List<PositionResponse>>>
{
    public Guid TripId { get; }

    public GetTripTrackQuery(Guid tripId) => TripId = tripId;
}