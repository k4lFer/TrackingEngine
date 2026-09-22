using System.Net;
using App.Interfaces.Ports.Geofences;
using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Geometry;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Geofences.Command.Delete;

public class DeleteGeofenceCommandHandler : ICommandHandler<DeleteGeofenceCommand, OutputPort<GeofenceResponse>>
{
    private readonly IGeofenceRepository _geofenceRepository;

    public DeleteGeofenceCommandHandler(IGeofenceRepository geofenceRepository)
    {
        _geofenceRepository = geofenceRepository;
    }

    public async Task<OutputPort<GeofenceResponse>> Handle(DeleteGeofenceCommand command, CancellationToken cancellationToken)
    {
        var entity = await _geofenceRepository.GetByIdAsync(command.Id, cancellationToken);
        if (entity is null)
        {
            return OutputPort<GeofenceResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró el geofence indicado.", "GEOFENCE_NOT_FOUND"));
        }

        _geofenceRepository.Remove(entity);
        await _geofenceRepository.SaveChangesAsync(cancellationToken);

        var response = new GeofenceResponse(
            entity.Id, entity.Code, entity.Name, entity.Kind.ToString(), entity.Priority,
            GeoJsonConverter.ToGeoJson(entity.Geometry), entity.MaxSpeedKmh, entity.Color, entity.Active);

        return OutputPort<GeofenceResponse>.Success(data: response, statusCode: HttpStatusCode.NoContent, message: "Geofence eliminado correctamente.");
    }
}