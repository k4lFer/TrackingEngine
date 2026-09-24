namespace App.Objects.Tracking.DTOs.Output.Response;

public record TripSummaryResponse(
    Guid Id,
    Guid VehicleId,
    string VehicleCode,
    Guid? RouteId,
    Guid? OriginGeofenceId,
    Guid? DestinationGeofenceId,
    string? OriginName,
    string? DestinationName,
    string? RouteCode,
    string? MaterialName,
    decimal? LoadTonnes,
    DateTime StartedAt,
    DateTime? EndedAt,
    decimal? DistanceKm,
    int? DurationS,
    string Status
);

public record TripDetailResponse(
    Guid Id,
    Guid VehicleId,
    string VehicleCode,
    Guid? RouteId,
    Guid? OriginGeofenceId,
    Guid? DestinationGeofenceId,
    string? OriginName,
    string? DestinationName,
    string? RouteName,
    string? MaterialName,
    decimal? LoadTonnes,
    DateTime StartedAt,
    DateTime? EndedAt,
    decimal? DistanceKm,
    int? DurationS,
    decimal? MaxSpeedKmh,
    int DeviationCount,
    int SpeedAlertCount,
    string Status,
    string? TrackGeoJson
);