using System.Net;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Vehicles.Query.GetById;

public class GetByIdVehicleQueryHandler : IQueryHandler<GetByIdVehicleQuery, OutputPort<VehicleStatusResponse>>
{
    private readonly IVehicleRepository _vehicleRepository;

    public GetByIdVehicleQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<OutputPort<VehicleStatusResponse>> Handle(GetByIdVehicleQuery query, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(query.Id, cancellationToken);
        if (vehicle is null)
        {
            return OutputPort<VehicleStatusResponse>.Failure(HttpStatusCode.NotFound, new MessageDto("VEHICLE_NOT_FOUND", "No se encontró el vehículo."));
        }

        return OutputPort<VehicleStatusResponse>.Success(data: new VehicleStatusResponse(
            vehicle.Id, vehicle.Code, vehicle.Plate,
            vehicle.CurrentState?.State.ToString() ?? "Offline",
            vehicle.CurrentState?.LastGeom?.Y, vehicle.CurrentState?.LastGeom?.X,
            vehicle.CurrentState?.LastReceivedAt, vehicle.CurrentState?.ActiveTripId));
    }
}