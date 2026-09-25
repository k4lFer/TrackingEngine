using System.Net;
using App.Interfaces.Ports;
using App.Interfaces.Ports.Devices;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Vehicles.Command.Delete;

public class DeleteVehicleCommandHandler : ICommandHandler<DeleteVehicleCommand, OutputPort<VehicleResponse>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVehicleCommandHandler(
        IVehicleRepository vehicleRepository,
        IDeviceRepository deviceRepository,
        IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _deviceRepository = deviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OutputPort<VehicleResponse>> Handle(DeleteVehicleCommand command, CancellationToken cancellationToken)
    {
        var entity = await _vehicleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (entity is null)
        {
            return OutputPort<VehicleResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("VEHICLE_NOT_FOUND", "No se encontró el vehículo indicado."));
        }

        var boundDevice = await _deviceRepository.GetBoundToVehicleAsync(entity.Id, cancellationToken);
        var identifier = boundDevice?.Identifier;
        boundDevice?.AssignToVehicle(null);

        await _vehicleRepository.RemoveAsync(entity, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        var response = new VehicleResponse(
            entity.Id, entity.Code, entity.Plate, entity.Brand, entity.Model, entity.Active,
            identifier, boundDevice is null ? null : (int?)boundDevice.Kind, boundDevice?.Model);
        return OutputPort<VehicleResponse>.Success(data: response, statusCode: HttpStatusCode.NoContent, message: "Vehículo eliminado correctamente.");
    }
}