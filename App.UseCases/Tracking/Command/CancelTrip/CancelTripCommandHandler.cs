using System.Net;
using App.Domain.Tracking.Entities;
using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Common.Enums;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Tracking.Command.CancelTrip;

public class CancelTripCommandHandler : ICommandHandler<CancelTripCommand, OutputPort<TripSummaryResponse>>
{
    private readonly ITrackingWriteRepository _trackingWrite;
    private readonly ITrackingReadRepository _trackingRead;

    public CancelTripCommandHandler(ITrackingWriteRepository trackingWrite, ITrackingReadRepository trackingRead)
    {
        _trackingWrite = trackingWrite;
        _trackingRead = trackingRead;
    }

    public async Task<OutputPort<TripSummaryResponse>> Handle(CancelTripCommand command, CancellationToken cancellationToken)
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
                new MessageDto("Solo se puede cancelar un viaje en curso.", "TRIP_NOT_ACTIVE"));
        }

        trip.Cancel();

        _trackingWrite.Update(trip);
        await _trackingWrite.SaveChangesAsync(cancellationToken);

        var detail = await _trackingRead.GetTripByIdAsync(trip.Id, cancellationToken);
        return OutputPort<TripSummaryResponse>.Success(
            data: detail is null ? null : ToSummary(detail),
            message: "Viaje cancelado correctamente.");
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