using System.Net;
using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetByIdTrip;

public class GetByIdTripQueryHandler : IQueryHandler<GetByIdTripQuery, OutputPort<TripDetailResponse>>
{
    private readonly ITrackingReadRepository _read;

    public GetByIdTripQueryHandler(ITrackingReadRepository read)
    {
        _read = read;
    }

    public async Task<OutputPort<TripDetailResponse>> Handle(GetByIdTripQuery query, CancellationToken cancellationToken)
    {
        var trip = await _read.GetTripByIdAsync(query.Id, cancellationToken);
        if (trip is null)
        {
            return OutputPort<TripDetailResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró el viaje indicado.", "TRIP_NOT_FOUND"));
        }

        return OutputPort<TripDetailResponse>.Success(data: trip);
    }
}