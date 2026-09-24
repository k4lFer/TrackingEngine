using System.Net;
using System.Text.Json;
using App.Domain.Routes.Entities;
using App.Interfaces.Ports.Geofences;
using App.Interfaces.Ports.Routes;
using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Result;
using App.UseCases.Routes.Common;
using Cortex.Mediator.Commands;

namespace App.UseCases.Routes.Command.Create;

public class CreateRouteCommandHandler : ICommandHandler<CreateRouteCommand, OutputPort<RouteResponse>>
{
    private readonly IRouteRepository _routeRepository;
    private readonly IRouteGeometryRepository _routeGeometryRepository;
    private readonly IGeofenceRepository _geofenceRepository;
    private readonly IRoutePlanner _routePlanner;
    private readonly ValhallaOptions _valhallaOptions;

    public CreateRouteCommandHandler(
        IRouteRepository routeRepository,
        IRouteGeometryRepository routeGeometryRepository,
        IGeofenceRepository geofenceRepository,
        IRoutePlanner routePlanner,
        ValhallaOptions valhallaOptions)
    {
        _routeRepository = routeRepository;
        _routeGeometryRepository = routeGeometryRepository;
        _geofenceRepository = geofenceRepository;
        _routePlanner = routePlanner;
        _valhallaOptions = valhallaOptions;
    }

    public async Task<OutputPort<RouteResponse>> Handle(CreateRouteCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Input;

        if (dto.Waypoints is null || dto.Waypoints.Count < 2)
        {
            return OutputPort<RouteResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("Se requieren al menos 2 puntos para crear la ruta.", "INVALID_ROUTE_GEOMETRY"));
        }

        if (await _routeRepository.ExistsAsync(dto.Code.Trim(), cancellationToken))
        {
            return OutputPort<RouteResponse>.Failure(
                HttpStatusCode.Conflict,
                new MessageDto("No se puede crear la ruta porque el código ya existe.", "ROUTE_CODE_EXISTS"));
        }

        if (dto.OriginGeofenceId.HasValue &&
            await _geofenceRepository.GetByIdAsync(dto.OriginGeofenceId.Value, cancellationToken) is null)
        {
            return OutputPort<RouteResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("La geocerca de origen indicada no existe.", "GEOFENCE_NOT_FOUND"));
        }

        if (dto.DestinationGeofenceId.HasValue &&
            await _geofenceRepository.GetByIdAsync(dto.DestinationGeofenceId.Value, cancellationToken) is null)
        {
            return OutputPort<RouteResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("La geocerca de destino indicada no existe.", "GEOFENCE_NOT_FOUND"));
        }

        var roads = await _routeGeometryRepository.GetAllActiveRoadsAsync(cancellationToken);
        var alternativesCount = Math.Clamp(dto.AlternativesCount ?? 2, 0, 2);
        var result = await RouteGeometryBuilder.BuildAsync(
            roads,
            dto.Waypoints,
            dto.ToleranceM,
            _valhallaOptions,
            _routePlanner,
            _valhallaOptions.Enabled,
            alternativesCount,
            cancellationToken);

        // La zona no está conectada por la red interna de la mina ni por la red
        // externa: solo quedaría una línea recta, por lo que se rechaza el alta.
        if (!result.UsedInternalNetwork && !result.ValhallaDurationS.HasValue)
        {
            return OutputPort<RouteResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto(
                    "No se encontró una ruta válida y accesible para los puntos indicados: la zona no está conectada por ninguna vía. Ajusta los puntos o elige otra zona.",
                    "ROUTE_NOT_ROUTABLE"));
        }

        var code = dto.Code.Trim();
        var name = dto.Name.Trim();
        var groupId = result.Alternatives.Count > 0 ? Guid.NewGuid() : (Guid?)null;

        var entity = TRoute.Create(
            code,
            name,
            result.LineString,
            dto.ToleranceM,
            dto.MaxSpeedKmh,
            dto.OriginGeofenceId,
            dto.DestinationGeofenceId,
            result.SpeedProfile is null ? null : JsonSerializer.Serialize(result.SpeedProfile),
            JsonSerializer.Serialize(dto.Waypoints),
            groupId,
            0);

        _routeRepository.Add(entity);

        for (int i = 0; i < result.Alternatives.Count; i++)
        {
            var alternative = TRoute.Create(
                AlternativeCode(code, i + 1),
                $"{name} (alternativa {i + 1})",
                result.Alternatives[i],
                dto.ToleranceM,
                dto.MaxSpeedKmh,
                dto.OriginGeofenceId,
                dto.DestinationGeofenceId,
                null,
                null,
                groupId,
                i + 1);
            _routeRepository.Add(alternative);
        }

        await _routeRepository.SaveChangesAsync(cancellationToken);

        var response = await _routeRepository.GetByIdAsync(entity.Id, cancellationToken);
        return OutputPort<RouteResponse>.Success(data: response, statusCode: HttpStatusCode.Created, message: "Ruta creada correctamente.");
    }

    private static string AlternativeCode(string baseCode, int rank)
    {
        const int maxLength = 50;
        var suffix = $"-ALT{rank}";
        return baseCode.Length + suffix.Length <= maxLength
            ? baseCode + suffix
            : baseCode[..(maxLength - suffix.Length)] + suffix;
    }
}