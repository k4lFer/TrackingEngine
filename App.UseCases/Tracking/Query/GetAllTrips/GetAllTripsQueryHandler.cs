using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetAllTrips;

public class GetAllTripsQueryHandler : IQueryHandler<GetAllTripsQuery, OutputPort<List<TripSummaryResponse>>>
{
    private readonly ITrackingReadRepository _read;

    public GetAllTripsQueryHandler(ITrackingReadRepository read)
    {
        _read = read;
    }

    public async Task<OutputPort<List<TripSummaryResponse>>> Handle(GetAllTripsQuery query, CancellationToken cancellationToken)
    {
        var trips = await _read.GetAllTripsAsync(cancellationToken);
        return OutputPort<List<TripSummaryResponse>>.Success(data: trips);
    }
}