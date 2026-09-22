namespace App.Objects.Simulation.DTOs.Output.Response;

/// <summary>
/// DTO de movimiento en vivo que el motor reenvía por SignalR a los tableros
/// (evento "VehicleMoved"). Lo publica el backend cuando algo reporta la
/// posición de un vehículo (p. ej. el script externo de dispositivos), no un
/// simulador interno.
/// </summary>
public record VehicleLiveDto(
    Guid VehicleId,
    string VehicleCode,
    Guid? TripId,
    double Lat,
    double Lon,
    decimal SpeedKmh,
    short HeadingDeg,
    double DistanceKm,
    double Progress,
    int EtaSec,
    decimal? LoadTonnes,
    string? MaterialName,
    string? OriginName,
    string? DestinationName,
    DateTime LastUpdated
);

/// <summary>
/// Estado del viaje que el motor reenvía por SignalR (evento "TripChanged").
/// </summary>
public record TripLiveDto(
    Guid TripId,
    Guid VehicleId,
    string VehicleCode,
    string Status,
    DateTime StartedAt,
    DateTime? EndedAt,
    decimal? DistanceKm,
    int? DurationS,
    decimal? LoadTonnes,
    string? MaterialName,
    string? OriginName,
    string? DestinationName
);