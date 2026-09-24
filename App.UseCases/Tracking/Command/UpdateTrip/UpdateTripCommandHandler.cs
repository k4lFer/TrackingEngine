using System.Net;
using App.Domain.Tracking.Entities;
using App.Interfaces.Ports.Materials;
using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Objects.Enums;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Tracking.Command.UpdateTrip;

public class UpdateTripCommandHandler : ICommandHandler<UpdateTripCommand, OutputPort<TripSummaryResponse>>
{
    private readonly ITrackingWriteRepository _trackingWrite;
    private readonly ITrackingReadRepository _trackingRead;
    private readonly IMaterialRepository _materialRepository;

    public UpdateTripCommandHandler(
        ITrackingWriteRepository trackingWrite,
        ITrackingReadRepository trackingRead,
        IMaterialRepository materialRepository)
    {
        _trackingWrite = trackingWrite;
        _trackingRead = trackingRead;
        _materialRepository = materialRepository;
    }

    public async Task<OutputPort<TripSummaryResponse>> Handle(UpdateTripCommand command, CancellationToken cancellationToken)
    {
        var trip = await _trackingWrite.GetTripEntityByIdAsync(command.Id, cancellationToken);
        if (trip is null)
        {
            return OutputPort<TripSummaryResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró el viaje indicado.", "TRIP_NOT_FOUND"));
        }

        if (trip.Status != TripStatus.Active)
        {
            return OutputPort<TripSummaryResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("Solo se puede editar un viaje en curso.", "TRIP_NOT_ACTIVE"));
        }

        var dto = command.Input;

        if (dto.LoadTonnes is < 0)
        {
            return OutputPort<TripSummaryResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("La carga no puede ser negativa.", "INVALID_LOAD"));
        }

        if (dto.MaterialId.HasValue)
        {
            var material = await _materialRepository.GetByIdAsync(dto.MaterialId.Value, cancellationToken);
            if (material is null)
            {
                return OutputPort<TripSummaryResponse>.Failure(
                    HttpStatusCode.BadRequest,
                    new MessageDto("El material indicado no existe.", "MATERIAL_NOT_FOUND"));
            }
        }

        trip.AdjustLoad(dto.MaterialId, dto.LoadTonnes);

        _trackingWrite.Update(trip);
        await _trackingWrite.SaveChangesAsync(cancellationToken);

        var detail = await _trackingRead.GetTripByIdAsync(trip.Id, cancellationToken);
        return OutputPort<TripSummaryResponse>.Success(
            data: detail is null ? null : ToSummary(detail),
            message: "Viaje actualizado correctamente.");
    }

    private static TripSummaryResponse ToSummary(TripDetailResponse d) => new(
        d.Id,
        d.VehicleId,
        d.VehicleCode,
        d.RouteId,
        d.OriginGeofenceId,
        d.DestinationGeofenceId,
        d.OriginName,
        d.DestinationName,
        null,
        d.MaterialName,
        d.LoadTonnes,
        d.StartedAt,
        d.EndedAt,
        d.DistanceKm,
        d.DurationS,
        d.Status);
}