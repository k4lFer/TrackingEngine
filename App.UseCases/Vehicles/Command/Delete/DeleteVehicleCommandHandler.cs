using System.Net;
using App.Interfaces.Ports;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Vehicles.Command.Delete;

public class DeleteVehicleCommandHandler : ICommandHandler<DeleteVehicleCommand, OutputPort<VehicleResponse>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVehicleCommandHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
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

        await _vehicleRepository.RemoveAsync(entity, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        var response = new VehicleResponse(entity.Id, entity.Code, entity.Plate, entity.Brand, entity.Model, entity.Active);
        return OutputPort<VehicleResponse>.Success(data: response, statusCode: HttpStatusCode.NoContent, message: "Vehículo eliminado correctamente.");
    }
}