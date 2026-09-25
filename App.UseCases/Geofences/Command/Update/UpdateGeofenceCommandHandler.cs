using System.Net;
using App.Interfaces.Ports.Geofences;
using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Utils.Geometry;
using App.Shared.Common.Enums;
using App.Shared.Common.Result;
using App.UseCases.Geofences.Common;
using Cortex.Mediator.Commands;

namespace App.UseCases.Geofences.Command.Update;

public class UpdateGeofenceCommandHandler : ICommandHandler<UpdateGeofenceCommand, OutputPort<GeofenceResponse>>
{
    private readonly IGeofenceRepository _geofenceRepository;

    public UpdateGeofenceCommandHandler(IGeofenceRepository geofenceRepository)
    {
        _geofenceRepository = geofenceRepository;
    }

    public async Task<OutputPort<GeofenceResponse>> Handle(UpdateGeofenceCommand command, CancellationToken cancellationToken)
    {
        var entity = await _geofenceRepository.GetByIdAsync(command.Id, cancellationToken);
        if (entity is null)
        {
            return OutputPort<GeofenceResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró el geofence indicado.", "GEOFENCE_NOT_FOUND"));
        }

        var dto = command.Input;
        entity.Update(
            dto.Name.Trim(),
            Enum.Parse<GeofenceKind>(dto.Kind, true),
            dto.Priority,
            GeofencePolygonBuilder.Build(dto.Polygon),
            dto.MaxSpeedKmh,
            dto.Active);

        _geofenceRepository.Update(entity);
        await _geofenceRepository.SaveChangesAsync(cancellationToken);

        var response = new GeofenceResponse(
            entity.Id, entity.Code, entity.Name, entity.Kind.ToString(), entity.Priority,
            GeoJsonConverter.ToGeoJson(entity.Geometry), entity.MaxSpeedKmh, entity.Color, entity.Active);

        return OutputPort<GeofenceResponse>.Success(data: response, statusCode: HttpStatusCode.OK, message: "Geofence actualizado correctamente.");
    }
}