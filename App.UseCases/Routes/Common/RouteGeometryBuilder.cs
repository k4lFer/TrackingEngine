using App.Domain.Routes.Entities;
using App.Domain.Routes.Routing;
using App.Interfaces.Ports.Routes;
using App.Objects.Shared.DTOs;
using App.Shared.Geometry;
using App.Shared.Objects.Enums;
using App.Shared.Routing;
using NetTopologySuite.Geometries;

namespace App.UseCases.Routes.Common;

public static class RouteGeometryBuilder
{
    /// <summary>
    /// Prioriza la red vial interna de la mina (los caminos dibujados en el mapa):
    /// si la red conecta los waypoints, la ruta sigue los caminos del yacimiento y
    /// Valhalla no se consulta. Valhalla solo entra como respaldo cuando la red
    /// interna no puede enrutar, para que los trazos fuera del polígono de la mina
    /// también sigan carreteras reales en vez de una línea recta.
    /// </summary>
    public static async Task<RouteBuildResult> BuildAsync(
        IReadOnlyList<TMineRoad> roads,
        IReadOnlyList<CoordinateDto> waypoints,
        int toleranceM,
        ValhallaOptions valhallaOptions,
        IRoutePlanner? externalPlanner,
        bool externalPlannerEnabled,
        CancellationToken cancellationToken)
    {
        // 1) Red interna: proyección a los caminos + Dijkstra por tiempo.
        double[]? speedProfile = null;
        var geoWaypoints = waypoints.Select(c => new GeoCoordinate(c.Lat, c.Lon)).ToList();
        var routedLine = roads.Count > 0 ? RoadNetworkRouter.Route(roads, geoWaypoints) : null;
        var usedRoadNetwork = routedLine is not null && routedLine.Coordinates.Length >= 2;

        // 2) La red interna conectó: su camino es el trazado, sin redondear esquinas
        //    para que la línea quede exactamente sobre el centro de las vías dibujadas.
        if (usedRoadNetwork)
        {
            speedProfile = RoadNetworkRouter.SpeedProfileFor(roads, routedLine!);
            return new RouteBuildResult(
                routedLine!,
                speedProfile,
                Array.Empty<LineString>(),
                RoadNetworkRouter.ManeuversFor(roads, routedLine!),
                null,
                UsedInternalNetwork: true);
        }

        // Sin ruta por la red interna: trazado por defecto en línea recta.
        var lineString = GeometryHelper.RoundCorners(
            waypoints.Select(c => new Coordinate(c.Lon, c.Lat)).ToArray(), toleranceM);

        // 3) Respaldo: Valhalla cuando la red interna no cubre los puntos. Se acepta
        //    si el tramo tiene extensión real o si el desvío propuesto es razonable
        //    (ruta local por carreteras cercanas) y se descarta si es exagerado.
        if (externalPlanner is null || !externalPlannerEnabled || waypoints.Count < 2)
            return new RouteBuildResult(lineString, speedProfile, Array.Empty<LineString>());

        var first = waypoints[0];
        var last = waypoints[^1];
        var plan = await externalPlanner.PlanAsync(
            RoutingProfile.Truck,
            new GeoCoordinate(first.Lat, first.Lon),
            new GeoCoordinate(last.Lat, last.Lon),
            cancellationToken);

        if (plan is null || plan.Points.Count < 2)
            return new RouteBuildResult(lineString, speedProfile, Array.Empty<LineString>());

        var spanKm = GeometryHelper.HaversineKm(first.Lat, first.Lon, last.Lat, last.Lon);
        var planKm = plan.DistanceMeters / 1000.0;

        if (spanKm < valhallaOptions.MinSpanKm && planKm > spanKm * valhallaOptions.MaxDetourRatio)
            return new RouteBuildResult(lineString, speedProfile, Array.Empty<LineString>());

        // La geometría de Valhalla se guarda y se devuelve TAL CUAL (los vértices del
        // shape ya siguen la red vial con ~30-40 m de separación). NO se simplifica:
        // cualquier simplificación/redondeo previo generaba "saltos" que se salían de
        // las curvas reales de las carreteras.
        var external = RawLineString(plan.Points);

        var alternatives = new List<LineString>(plan.Alternatives.Count);
        foreach (var alt in plan.Alternatives)
        {
            if (alt.Points.Count < 2)
                continue;
            alternatives.Add(RawLineString(alt.Points));
        }

        return new RouteBuildResult(
            external,
            null,
            alternatives,
            plan.Maneuvers,
            plan.RawShape,
            UsedInternalNetwork: false,
            ValhallaDurationS: plan.DurationSeconds);
    }

    /// <summary>
    /// Convierte los puntos decodificados de Valhalla (que ya recorren la geometría
    /// real de las vías) a un LineString 4326 sin ningún tipo de simplificación ni
    /// redondeo, para preservar fielmente el trazado de las carreteras.
    /// </summary>
    private static LineString RawLineString(IReadOnlyList<GeoCoordinate> points)
    {
        var coords = points.Select(p => new Coordinate(p.Lon, p.Lat)).ToArray();
        return new LineString(coords) { SRID = 4326 };
    }
}

/// <summary>
/// Resultado del enrutado: geometría principal, perfil de velocidad, variantes
/// alternas y los datos de navegación que el proveedor devuelve (instrucciones
/// paso a paso, shape crudo de Valhalla y el tiempo real calculado).
/// </summary>
public sealed record RouteBuildResult(
    LineString LineString,
    double[]? SpeedProfile,
    IReadOnlyList<LineString> Alternatives,
    IReadOnlyList<RouteManeuver>? Maneuvers = null,
    string? RawShape = null,
    bool UsedInternalNetwork = false,
    double? ValhallaDurationS = null);