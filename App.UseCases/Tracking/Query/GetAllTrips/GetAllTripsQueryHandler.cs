using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using App.UseCases.Tracking.Query.Filter;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetAllTrips;

public class GetAllTripsQueryHandler : IQueryHandler<GetAllTripsQuery, OutputPort<QueryResult<TripSummaryResponse>>>
{
    private readonly ITrackingReadRepository _read;

    public GetAllTripsQueryHandler(ITrackingReadRepository read)
    {
        _read = read;
    }

    public async Task<OutputPort<QueryResult<TripSummaryResponse>>> Handle(GetAllTripsQuery query, CancellationToken cancellationToken)
    {
        var filter = new FilterAllTrips
        {
            Search = query.Filter.Search,
            Status = query.Filter.Status,
            FromDate = query.Filter.FromDate,
            ToDate = query.Filter.ToDate,
        };

        var results = await _read.GetTripsPagedAsync(
            query.Filter.NumberPage,
            query.Filter.PageSize,
            filter,
            cancellationToken);

        return OutputPort<QueryResult<TripSummaryResponse>>.Success(data: results);
    }
}