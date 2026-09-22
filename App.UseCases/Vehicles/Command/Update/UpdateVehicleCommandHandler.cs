using App.Objects.Vehicles.DTOs.Output.Response;
using App.Interfaces.Ports;
using App.Interfaces.Ports.Vehicles;
using App.Shared.Result;
using Cortex.Mediator.Commands;
using System.Net;

namespace App.UseCases.Vehicles.Command.Update;

public class UpdateVehicleCommandHandler : ICommandHandler<UpdateVehicleCommand, OutputPort<VehicleResponse>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVehicleCommandHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OutputPort<VehicleResponse>> Handle(UpdateVehicleCommand command, CancellationToken cancellationToken)
    {
        var entity = await _vehicleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (entity is null)
        {
            return OutputPort<VehicleResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("VEHICLE_NOT_FOUND", "No se encontró el vehículo indicado."));
        }

var dto = command.Input;
        entity.Update(dto.Plate.Trim(), dto.Brand.Trim(), dto.Model.Trim(), dto.Active);

        await _unitOfWork.SaveChanges(cancellationToken);

        var response = new VehicleResponse(entity.Id, entity.Code, entity.Plate, entity.Brand, entity.Model, entity.Active);
        return OutputPort<VehicleResponse>.Success(data: response, statusCode: HttpStatusCode.OK, message: "Vehículo actualizado correctamente.");
    }
}
