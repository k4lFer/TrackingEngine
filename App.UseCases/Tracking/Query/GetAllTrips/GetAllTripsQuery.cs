using App.Objects.Tracking.DTOs.Input.Query;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetAllTrips;

public class GetAllTripsQuery : IQuery<OutputPort<QueryResult<TripSummaryResponse>>>
{
    public TripFilterDto Filter { get; }

    public GetAllTripsQuery(TripFilterDto filter)
    {
        Filter = filter;
    }
}