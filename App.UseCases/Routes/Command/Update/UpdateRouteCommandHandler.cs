using System.Net;
using System.Text.Json;
using App.Domain.Routes.Entities;
using App.Interfaces.Ports.Geofences;
using App.Interfaces.Ports.Routes;
using App.Objects.Routes.DTOs.Input.Command;
using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Result;
using App.Shared.Validation;
using App.UseCases.Routes.Common;
using Cortex.Mediator.Commands;

namespace App.UseCases.Routes.Command.Update;

public class UpdateRouteCommandHandler : ICommandHandler<UpdateRouteCommand, OutputPort<RouteResponse>>
{
    private const int MaxCodeLength = 50;

    private readonly IRouteRepository _routeRepository;
    private readonly IRouteGeometryRepository _routeGeometryRepository;
    private readonly IGeofenceRepository _geofenceRepository;
    private readonly IRoutePlanner _routePlanner;
    private readonly ValhallaOptions _valhallaOptions;
    private readonly IInputValidator<UpdateRouteRequest> _validator;

    public UpdateRouteCommandHandler(
        IRouteRepository routeRepository,
        IRouteGeometryRepository routeGeometryRepository,
        IGeofenceRepository geofenceRepository,
        IRoutePlanner routePlanner,
        ValhallaOptions valhallaOptions,
        IInputValidator<UpdateRouteRequest> validator)
    {
        _routeRepository = routeRepository;
        _routeGeometryRepository = routeGeometryRepository;
        _geofenceRepository = geofenceRepository;
        _routePlanner = routePlanner;
        _valhallaOptions = valhallaOptions;
        _validator = validator;
    }

    public async Task<OutputPort<RouteResponse>> Handle(UpdateRouteCommand command, CancellationToken cancellationToken)
    {
        if (!await _validator.ValidateAsync(command.Input, cancellationToken))
        {
            return OutputPort<RouteResponse>.Failure(_validator.StatusCode, _validator.Messages.ToArray());
        }

        var entity = await _routeRepository.GetByIdRawAsync(command.Id, cancellationToken);
        if (entity is null)
        {
            return OutputPort<RouteResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró la ruta indicada.", "ROUTE_NOT_FOUND"));
        }

        if (entity.AlternativeRank != 0)
        {
            return OutputPort<RouteResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("Solo se puede editar la ruta principal.", "INVALID_ROUTE_TARGET"));
        }

        var dto = command.Input;

        if (dto.Waypoints is null || dto.Waypoints.Count < 2)
        {
            return OutputPort<RouteResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("Se requieren al menos 2 puntos para actualizar la ruta.", "INVALID_ROUTE_GEOMETRY"));
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

        if (!result.UsedInternalNetwork && !result.ValhallaDurationS.HasValue)
        {
            return OutputPort<RouteResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto(
                    "No se encontró una ruta válida y accesible para los puntos indicados: la zona no está conectada por ninguna vía. Ajusta los puntos o elige otra zona.",
                    "ROUTE_NOT_ROUTABLE"));
        }

        var groupId = entity.RouteGroupId;
        if (result.Alternatives.Count > 0 && groupId is null)
        {
            groupId = Guid.NewGuid();
            entity.AssignGroup(groupId);
        }

        entity.Update(
            dto.Name.Trim(),
            result.LineString,
            dto.ToleranceM,
            dto.MaxSpeedKmh,
            dto.OriginGeofenceId,
            dto.DestinationGeofenceId,
            result.SpeedProfile is null ? null : JsonSerializer.Serialize(result.SpeedProfile),
            JsonSerializer.Serialize(dto.Waypoints),
            dto.Active);

        if (entity.RouteGroupId is Guid existingGroup)
        {
            var group = await _routeRepository.GetGroupRawAsync(existingGroup, cancellationToken);
            foreach (var alternative in group.Where(r => r.Id != entity.Id))
            {
                _routeRepository.Remove(alternative);
            }
        }

        for (int i = 0; i < result.Alternatives.Count; i++)
        {
            var alternative = TRoute.Create(
                AlternativeCode(entity.Code, i + 1),
                $"{entity.Name} (alternativa {i + 1})",
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
        return OutputPort<RouteResponse>.Success(data: response, statusCode: HttpStatusCode.OK, message: "Ruta actualizada correctamente.");
    }

    private static string AlternativeCode(string baseCode, int rank)
    {
        var suffix = $"-ALT{rank}";
        return baseCode.Length + suffix.Length <= MaxCodeLength
            ? baseCode + suffix
            : baseCode[..(MaxCodeLength - suffix.Length)] + suffix;
    }
}