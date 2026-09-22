using App.Shared.Geometry;
using App.Shared.Objects.Enums;
using App.Shared.Routing;

namespace App.Interfaces.Ports.Routes;

public interface IRoutePlanner
{
    /// <summary>Devuelve la ruta por red vial (puntos lat/lon), null si no hay camino.</summary>
    Task<RoutePlan?> PlanAsync(
        RoutingProfile profile,
        GeoCoordinate from,
        GeoCoordinate to,
        CancellationToken cancellationToken = default);
}

public sealed record RoutePlan(
    IReadOnlyList<GeoCoordinate> Points,
    double DistanceMeters,
    double DurationSeconds)
{
    /// <summary>Variantes alternativas de la misma ruta (rank 1..n), si el proveedor las devuelve.</summary>
    public IReadOnlyList<RoutePlan> Alternatives { get; init; } = Array.Empty<RoutePlan>();

    /// <summary>
    /// Instrucciones paso a paso. Cuando la ruta proviene de Valhalla son las
    /// <c>maneuvers</c> originales; cuando proviene de la red interna de la mina
    /// se generan a partir de los giros de la geometría.
    /// </summary>
    public IReadOnlyList<RouteManeuver>? Maneuvers { get; init; }

    /// <summary>
    /// Forma comprimida en Polyline6 (string) que devuelve Valhalla en
    /// <c>trip.legs[0].shape</c>. Es el dato crudo de Valhalla; la geometría
    /// decodificada y simplificada se expone aparte como GeoJSON.
    /// </summary>
    public string? RawShape { get; init; }
}