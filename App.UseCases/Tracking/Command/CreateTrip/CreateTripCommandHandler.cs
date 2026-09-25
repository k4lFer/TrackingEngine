using System.Net;
using App.Domain.Tracking.Entities;
using App.Interfaces.Ports.Geofences;
using App.Interfaces.Ports.Materials;
using App.Interfaces.Ports.Routes;
using App.Interfaces.Ports.Tracking;
using App.Interfaces.Ports.Vehicles;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Tracking.Command.CreateTrip;

public class CreateTripCommandHandler : ICommandHandler<CreateTripCommand, OutputPort<TripSummaryResponse>>
{
    private readonly ITrackingWriteRepository _trackingWrite;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IGeofenceRepository _geofenceRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IMaterialRepository _materialRepository;

    public CreateTripCommandHandler(
        ITrackingWriteRepository trackingWrite,
        IVehicleRepository vehicleRepository,
        IGeofenceRepository geofenceRepository,
        IRouteRepository routeRepository,
        IMaterialRepository materialRepository)
    {
        _trackingWrite = trackingWrite;
        _vehicleRepository = vehicleRepository;
        _geofenceRepository = geofenceRepository;
        _routeRepository = routeRepository;
        _materialRepository = materialRepository;
    }

    public async Task<OutputPort<TripSummaryResponse>> Handle(CreateTripCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Input;

        if (dto.LoadTonnes is < 0)
        {
            return OutputPort<TripSummaryResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("La carga no puede ser negativa.", "INVALID_LOAD"));
        }

        var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            return OutputPort<TripSummaryResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("El vehículo indicado no existe.", "VEHICLE_NOT_FOUND"));
        }

        if (!vehicle.Active)
        {
            return OutputPort<TripSummaryResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("El vehículo está inactivo y no puede hacer viajes.", "VEHICLE_INACTIVE"));
        }

        var activeTrip = await _trackingWrite.GetActiveTripByVehicleAsync(dto.VehicleId, cancellationToken);
        if (activeTrip is not null)
        {
            return OutputPort<TripSummaryResponse>.Failure(
                HttpStatusCode.Conflict,
                new MessageDto("El vehículo ya tiene un viaje en curso.", "TRIP_ALREADY_ACTIVE"));
        }

        var origin = dto.OriginGeofenceId.HasValue
            ? await _geofenceRepository.GetByIdAsync(dto.OriginGeofenceId.Value, cancellationToken)
            : null;
        if (dto.OriginGeofenceId.HasValue && origin is null)
        {
            return OutputPort<TripSummaryResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("La geocerca de origen indicada no existe.", "ORIGIN_NOT_FOUND"));
        }

        var destination = dto.DestinationGeofenceId.HasValue
            ? await _geofenceRepository.GetByIdAsync(dto.DestinationGeofenceId.Value, cancellationToken)
            : null;
        if (dto.DestinationGeofenceId.HasValue && destination is null)
        {
            return OutputPort<TripSummaryResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("La geocerca de destino indicada no existe.", "DESTINATION_NOT_FOUND"));
        }

        if (origin is not null && destination is not null && origin.Id == destination.Id)
        {
            return OutputPort<TripSummaryResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("El origen y el destino deben ser geocercas distintas.", "SAME_GEOFENCE"));
        }

        var route = dto.RouteId.HasValue
            ? await _routeRepository.GetByIdRawAsync(dto.RouteId.Value, cancellationToken)
            : null;
        if (dto.RouteId.HasValue && route is null)
        {
            return OutputPort<TripSummaryResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("La ruta indicada no existe.", "ROUTE_NOT_FOUND"));
        }

        var material = dto.MaterialId.HasValue
            ? await _materialRepository.GetByIdAsync(dto.MaterialId.Value, cancellationToken)
            : null;
        if (dto.MaterialId.HasValue && material is null)
        {
            return OutputPort<TripSummaryResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("El material indicado no existe.", "MATERIAL_NOT_FOUND"));
        }

        var trip = TTrip.Start(
            vehicle.Id,
            dto.OriginGeofenceId,
            dto.DestinationGeofenceId,
            dto.RouteId,
            dto.MaterialId,
            dto.LoadTonnes);

        _trackingWrite.Add(trip);
        await _trackingWrite.SaveChangesAsync(cancellationToken);

        var response = new TripSummaryResponse(
            trip.Id,
            trip.VehicleId,
            vehicle.Code,
            route?.Id,
            dto.OriginGeofenceId,
            dto.DestinationGeofenceId,
            origin?.Name,
            destination?.Name,
            route?.Code,
            material?.Name,
            trip.LoadTonnes,
            trip.StartedAt,
            null,
            null,
            null,
            "Active");

        return OutputPort<TripSummaryResponse>.Success(
            data: response,
            statusCode: HttpStatusCode.Created,
            message: "Viaje iniciado correctamente.");
    }
}