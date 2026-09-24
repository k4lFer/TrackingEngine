using System.Net;
using App.Interfaces.Ports.Devices;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Vehicles.Query.GetById;

public class GetByIdVehicleQueryHandler : IQueryHandler<GetByIdVehicleQuery, OutputPort<VehicleStatusResponse>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDeviceRepository _deviceRepository;

    public GetByIdVehicleQueryHandler(IVehicleRepository vehicleRepository, IDeviceRepository deviceRepository)
    {
        _vehicleRepository = vehicleRepository;
        _deviceRepository = deviceRepository;
    }

    public async Task<OutputPort<VehicleStatusResponse>> Handle(GetByIdVehicleQuery query, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(query.Id, cancellationToken);
        if (vehicle is null)
        {
            return OutputPort<VehicleStatusResponse>.Failure(HttpStatusCode.NotFound, new MessageDto("VEHICLE_NOT_FOUND", "No se encontró el vehículo."));
        }

        var device = await _deviceRepository.GetBoundToVehicleAsync(vehicle.Id, cancellationToken);

        return OutputPort<VehicleStatusResponse>.Success(data: new VehicleStatusResponse(
            vehicle.Id, vehicle.Code, vehicle.Plate, vehicle.Brand, vehicle.Model, vehicle.Active,
            device?.Identifier, device is null ? null : (int?)device.Kind, device?.Model,
            vehicle.CurrentState?.State.ToString() ?? "Offline",
            vehicle.CurrentState?.LastGeom?.Y, vehicle.CurrentState?.LastGeom?.X,
            vehicle.CurrentState?.LastReceivedAt, vehicle.CurrentState?.ActiveTripId));
    }
}