using System.Net;
using App.Domain.Vehicles.Entities;
using App.Interfaces.Ports;
using App.Interfaces.Ports.Devices;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Common.Enums;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Vehicles.Command.Create;

public class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand, OutputPort<VehicleResponse>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVehicleCommandHandler(
        IVehicleRepository vehicleRepository,
        IDeviceRepository deviceRepository,
        IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _deviceRepository = deviceRepository;
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

        var identifier = dto.DeviceIdentifier?.Trim();
        if (!string.IsNullOrEmpty(identifier))
        {
            var device = await _deviceRepository.GetByIdentifierAsync(identifier, cancellationToken);
            if (device is null)
            {
                device = TDevice.Create(identifier, ResolveKind(dto.DeviceKind, DeviceKind.Traccar), dto.DeviceModel);
                await _deviceRepository.AddAsync(device, cancellationToken);
            }
            else if (device.VehicleId.HasValue && device.VehicleId != entity.Id)
            {
                return OutputPort<VehicleResponse>.Failure(
                    HttpStatusCode.Conflict,
                    new MessageDto("DEVICE_ALREADY_ASSIGNED", $"El dispositivo {identifier} ya está vinculado a otro vehículo."));
            }
            else
            {
                device.SetDetails(ResolveKind(dto.DeviceKind, device.Kind), dto.DeviceModel);
            }

            device.AssignToVehicle(entity.Id);
        }

        await _unitOfWork.SaveChanges(cancellationToken);

        var deviceInfo = await _deviceRepository.GetBoundToVehicleAsync(entity.Id, cancellationToken);
        var response = new VehicleResponse(
            entity.Id, entity.Code, entity.Plate, entity.Brand, entity.Model, entity.Active,
            deviceInfo?.Identifier, deviceInfo is null ? null : (int?)deviceInfo.Kind, deviceInfo?.Model);
        return OutputPort<VehicleResponse>.Success(data: response, statusCode: HttpStatusCode.Created, message: "Vehículo creado correctamente.");
    }

    private static DeviceKind ResolveKind(int? value, DeviceKind fallback)
    {
        if (value is int k && Enum.IsDefined(typeof(DeviceKind), k))
        {
            return (DeviceKind)k;
        }

        return fallback;
    }
}