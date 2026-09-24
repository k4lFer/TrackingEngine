using System.Net;
using App.Interfaces.Ports.Routes;
using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Geometry;
using App.Shared.Result;
using App.UseCases.Routes.Common;
using Cortex.Mediator.Commands;

namespace App.UseCases.Routes.Command.Preview;

public class PreviewRouteCommandHandler : ICommandHandler<PreviewRouteCommand, OutputPort<RouteResponse>>
{
    private readonly IRouteGeometryRepository _routeGeometryRepository;
    private readonly IRoutePlanner _routePlanner;
    private readonly ValhallaOptions _valhallaOptions;

    public PreviewRouteCommandHandler(
        IRouteGeometryRepository routeGeometryRepository,
        IRoutePlanner routePlanner,
        ValhallaOptions valhallaOptions)
    {
        _routeGeometryRepository = routeGeometryRepository;
        _routePlanner = routePlanner;
        _valhallaOptions = valhallaOptions;
    }

    public async Task<OutputPort<RouteResponse>> Handle(PreviewRouteCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Input;

        if (dto.Waypoints is null || dto.Waypoints.Count < 2)
        {
            return OutputPort<RouteResponse>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto("Se requieren al menos 2 puntos para trazar la ruta.", "INVALID_ROUTE_GEOMETRY"));
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

        var geoJson = GeoJsonConverter.ToGeoJson(result.LineString);
        var km = GeometryHelper.LengthKm(result.LineString);

        var alternatives = result.Alternatives
            .Select((line, i) =>
            {
                var altKm = GeometryHelper.LengthKm(line);
                return new RouteAlternativeResponse(
                    Guid.Empty,
                    string.Empty,
                    $"Alternativa {i + 1}",
                    i + 1,
                    GeoJsonConverter.ToGeoJson(line),
                    altKm,
                    EstimateDurationS(dto.MaxSpeedKmh, altKm));
            })
            .ToList();

        var durationS = result.ValhallaDurationS ?? EstimateDurationS(dto.MaxSpeedKmh, km);

        // mine: red interna de la mina. valhalla: red vial externa.
        // none: fallback en línea recta (red interna no conectó y no hubo
        // trayectoria externa) — el front lo trata como zona no accesible.
        var provider = result.UsedInternalNetwork
            ? "mine"
            : result.ValhallaDurationS.HasValue
                ? "valhalla"
                : "none";

        var response = new RouteResponse(
            Guid.Empty,
            string.Empty,
            string.Empty,
            geoJson,
            dto.ToleranceM,
            dto.MaxSpeedKmh,
            true,
            km,
            durationS,
            null,
            null,
            null,
            null,
            alternatives,
            provider,
            result.Maneuvers?
                .Select(m => new RouteInstructionResponse(m.Type, m.Instruction, m.StreetNames, m.LengthKm, m.TimeSeconds))
                .ToList(),
            result.RawShape,
            result.ValhallaDurationS.HasValue
                ? new ValhallaSummaryResponse(
                    result.ValhallaDurationS.Value,
                    GeometryHelper.LengthKm(result.LineString),
                    null)
                : null);

        return OutputPort<RouteResponse>.Success(data: response, statusCode: HttpStatusCode.OK, message: "Vista previa de ruta calculada.");
    }

    private const int DefaultMaxSpeedKmh = 40;

    private static double EstimateDurationS(int? maxSpeedKmh, double km)
    {
        if (km <= 0) return 0;
        double speed = maxSpeedKmh is > 0 ? maxSpeedKmh.Value : DefaultMaxSpeedKmh;
        return km / speed * 3600;
    }
}