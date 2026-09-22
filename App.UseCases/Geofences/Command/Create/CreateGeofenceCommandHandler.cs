using System.Net;
using App.Domain.Geofences.Entities;
using App.Interfaces.Ports.Geofences;
using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Geometry;
using App.Shared.Objects.Enums;
using App.Shared.Result;
using App.UseCases.Geofences.Common;
using Cortex.Mediator.Commands;

namespace App.UseCases.Geofences.Command.Create;

public class CreateGeofenceCommandHandler : ICommandHandler<CreateGeofenceCommand, OutputPort<GeofenceResponse>>
{
    private readonly IGeofenceRepository _geofenceRepository;

    public CreateGeofenceCommandHandler(IGeofenceRepository geofenceRepository)
    {
        _geofenceRepository = geofenceRepository;
    }

    public async Task<OutputPort<GeofenceResponse>> Handle(CreateGeofenceCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Input;

        if (await _geofenceRepository.ExistsAsync(dto.Code.Trim(), cancellationToken))
        {
            return OutputPort<GeofenceResponse>.Failure(
                HttpStatusCode.Conflict,
                new MessageDto("No se puede crear el geofence porque el código ya existe.", "GEOFENCE_CODE_EXISTS"));
        }

        var entity = TGeofence.Create(
            dto.Code.Trim(),
            dto.Name.Trim(),
            Enum.Parse<GeofenceKind>(dto.Kind, true),
            dto.Priority,
            GeofencePolygonBuilder.Build(dto.Polygon),
            dto.MaxSpeedKmh,
            dto.Color);

        _geofenceRepository.Add(entity);
        await _geofenceRepository.SaveChangesAsync(cancellationToken);

        var response = new GeofenceResponse(
            entity.Id, entity.Code, entity.Name, entity.Kind.ToString(), entity.Priority,
            GeoJsonConverter.ToGeoJson(entity.Geometry), entity.MaxSpeedKmh, entity.Color, entity.Active);

        return OutputPort<GeofenceResponse>.Success(data: response, statusCode: HttpStatusCode.Created, message: "Geofence creado correctamente.");
    }
}