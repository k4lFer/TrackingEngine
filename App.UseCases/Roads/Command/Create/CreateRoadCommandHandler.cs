using System.Net;
using App.Domain.Routes.Entities;
using App.Interfaces.Ports.Roads;
using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Utils.Geometry;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace App.UseCases.Roads.Command.Create;

public class CreateRoadCommandHandler : ICommandHandler<CreateRoadCommand, OutputPort<RoadResponse>>
{
    private readonly IRoadRepository _roadRepository;
    private static readonly GeometryFactory _gf = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public CreateRoadCommandHandler(IRoadRepository roadRepository)
    {
        _roadRepository = roadRepository;
    }

    public async Task<OutputPort<RoadResponse>> Handle(CreateRoadCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Input;

        if (dto.Waypoints is null || dto.Waypoints.Count < 2)
        {
            return OutputPort<RoadResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("Se requieren al menos 2 puntos para crear un camino.", "INVALID_ROAD_GEOMETRY"));
        }

        var coords = dto.Waypoints.Select(c => new Coordinate(c.Lon, c.Lat)).ToArray();
        var line = _gf.CreateLineString(coords);
        if (line.Length == 0 || line.Coordinates.Length < 2)
        {
            return OutputPort<RoadResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("La geometría del camino es inválida.", "INVALID_ROAD_GEOMETRY"));
        }

        var code = string.IsNullOrWhiteSpace(dto.Code)
            ? Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()
            : dto.Code.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Code) && await _roadRepository.ExistsAsync(code, cancellationToken))
        {
            return OutputPort<RoadResponse>.Failure(
                HttpStatusCode.Conflict,
                new MessageDto("No se puede crear el camino porque el código ya existe.", "ROAD_CODE_EXISTS"));
        }

        var entity = TMineRoad.Create(
            code,
            string.IsNullOrWhiteSpace(dto.Name) ? "Camino" : dto.Name.Trim(),
            line,
            dto.MaxSpeedKmh);

        _roadRepository.Add(entity);
        await _roadRepository.SaveChangesAsync(cancellationToken);

        var response = new RoadResponse(
            entity.Id, entity.Code, entity.Name,
            GeoJsonConverter.ToGeoJson(entity.Geometry),
            entity.MaxSpeedKmh, entity.Active, GeometryHelper.LengthKm(entity.Geometry));

        return OutputPort<RoadResponse>.Success(data: response, statusCode: HttpStatusCode.Created, message: "Camino creado correctamente.");
    }
}