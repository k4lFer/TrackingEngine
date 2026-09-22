using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetByIdTrip;

public class GetByIdTripQuery : IQuery<OutputPort<TripDetailResponse>>
{
    public Guid Id { get; }

    public GetByIdTripQuery(Guid id) => Id = id;
}