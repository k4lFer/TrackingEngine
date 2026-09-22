using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetAllTrips;

public class GetAllTripsQuery : IQuery<OutputPort<List<TripSummaryResponse>>>
{
}