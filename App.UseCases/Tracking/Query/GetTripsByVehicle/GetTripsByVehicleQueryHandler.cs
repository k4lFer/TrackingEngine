using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetTripsByVehicle;

public class GetTripsByVehicleQueryHandler : IQueryHandler<GetTripsByVehicleQuery, OutputPort<List<TripSummaryResponse>>>
{
    private readonly ITrackingReadRepository _read;

    public GetTripsByVehicleQueryHandler(ITrackingReadRepository read)
    {
        _read = read;
    }

    public async Task<OutputPort<List<TripSummaryResponse>>> Handle(GetTripsByVehicleQuery query, CancellationToken cancellationToken)
    {
        var trips = await _read.GetTripsAsync(query.VehicleId, cancellationToken);
        return OutputPort<List<TripSummaryResponse>>.Success(data: trips);
    }
}