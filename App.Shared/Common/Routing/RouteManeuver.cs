namespace App.Shared.Common.Routing;

/// <summary>
/// Instrucción de navegación (maneuver) con la misma forma que las que devuelve
/// Valhalla en <c>trip.legs[].maneuvers</c>. Longitud y tiempo corresponden al
/// tramo entre esta maniobra y la siguiente. Cuando la ruta sale de la red
/// interna de la mina se exponen tal cual las devuelve Valhalla; cuando proviene
/// de un camino interno se generan a partir de los giros de la geometría.
/// </summary>
public sealed record RouteManeuver(
    int Type,
    string? Instruction,
    IReadOnlyList<string> StreetNames,
    double LengthKm,
    double TimeSeconds);