namespace App.Objects.Routes.DTOs.Output.Response;

public sealed record RouteAlternativeResponse(
    Guid Id,
    string Code,
    string Name,
    int AlternativeRank,
    string? GeoJsonLine,
    double DistanceKm,
    double EstimatedDurationS);

/// <summary>
/// Maniobra de navegación. Es la <c>maneuvers</c> de Valhalla (cuando la ruta
/// proviene de Valhalla) o una instrucción generada sobre la red interna de la mina.
/// </summary>
public sealed record RouteInstructionResponse(
    int Type,
    string? Instruction,
    IReadOnlyList<string> StreetNames,
    double DistanceKm,
    double TimeSeconds);

/// <summary>Resumen de ruta equivalente al <c>trip.summary</c> de Valhalla.</summary>
public sealed record ValhallaSummaryResponse(
    double TimeSeconds,
    double LengthKm,
    double? Cost);

public record RouteResponse(
    Guid Id,
    string Code,
    string Name,
    string GeoJsonLine,
    int ToleranceM,
    int? MaxSpeedKmh,
    bool Active,
    double DistanceKm,
    double EstimatedDurationS,
    Guid? OriginGeofenceId,
    string? OriginGeofenceName,
    Guid? DestinationGeofenceId,
    string? DestinationGeofenceName,
    IReadOnlyList<RouteAlternativeResponse>? Alternatives = null,
    string? Provider = null,
    IReadOnlyList<RouteInstructionResponse>? Instructions = null,
    string? EncodedShape = null,
    ValhallaSummaryResponse? ValhallaSummary = null
);