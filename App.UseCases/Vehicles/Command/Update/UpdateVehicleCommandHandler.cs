using System.Net;
using App.Domain.Vehicles.Entities;
using App.Interfaces.Ports;
using App.Interfaces.Ports.Devices;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Common.Enums;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Vehicles.Command.Update;

public class UpdateVehicleCommandHandler : ICommandHandler<UpdateVehicleCommand, OutputPort<VehicleResponse>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVehicleCommandHandler(
        IVehicleRepository vehicleRepository,
        IDeviceRepository deviceRepository,
        IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _deviceRepository = deviceRepository;
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

        var identifier = dto.DeviceIdentifier?.Trim();

        // Desvincular: libera el dispositivo y apaga el vehículo (Offline sin posición).
        if (string.IsNullOrEmpty(identifier))
        {
            var current = await _deviceRepository.GetBoundToVehicleAsync(entity.Id, cancellationToken);
            if (current is not null)
            {
                current.AssignToVehicle(null);
            }

            entity.GoOffline();
        }
        else
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

            // Si deja de reportar un dispositivo anterior, se libera.
            var previous = await _deviceRepository.GetBoundToVehicleAsync(entity.Id, cancellationToken);
            if (previous is not null && previous.Id != device.Id)
            {
                previous.AssignToVehicle(null);
            }

            device.AssignToVehicle(entity.Id);
        }

        await _unitOfWork.SaveChanges(cancellationToken);

        var deviceInfo = await _deviceRepository.GetBoundToVehicleAsync(entity.Id, cancellationToken);
        var response = new VehicleResponse(
            entity.Id, entity.Code, entity.Plate, entity.Brand, entity.Model, entity.Active,
            deviceInfo?.Identifier, deviceInfo is null ? null : (int?)deviceInfo.Kind, deviceInfo?.Model);
        return OutputPort<VehicleResponse>.Success(data: response, statusCode: HttpStatusCode.OK, message: "Vehículo actualizado correctamente.");
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