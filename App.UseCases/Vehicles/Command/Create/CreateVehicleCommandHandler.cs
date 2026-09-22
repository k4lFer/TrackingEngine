using System.Net;
using App.Domain.Vehicles.Entities;
using App.Interfaces.Ports;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Vehicles.Command.Create;

public class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand, OutputPort<VehicleResponse>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVehicleCommandHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OutputPort<VehicleResponse>> Handle(CreateVehicleCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Input;

        var entity = TVehicle.Create
        (
            dto.Code.Trim(),
            dto.Plate.Trim(),
            dto.Brand.Trim(),
            dto.Model.Trim()
        );

        await _vehicleRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        var response = new VehicleResponse(entity.Id, entity.Code, entity.Plate, entity.Brand, entity.Model, entity.Active);
        return OutputPort<VehicleResponse>.Success(data: response, statusCode: HttpStatusCode.Created, message: "Vehículo creado correctamente.");
    }
}