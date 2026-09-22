using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetTripsByVehicle;

public class GetTripsByVehicleQuery : IQuery<OutputPort<List<TripSummaryResponse>>>
{
    public Guid VehicleId { get; }

    public GetTripsByVehicleQuery(Guid vehicleId) => VehicleId = vehicleId;
}