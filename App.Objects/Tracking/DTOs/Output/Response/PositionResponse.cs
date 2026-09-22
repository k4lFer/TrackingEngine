namespace App.Objects.Tracking.DTOs.Output.Response;

public record PositionResponse(
    Guid Id,
    Guid VehicleId,
    DateTime RecordedAt,
    double Lat,
    double Lon,
    decimal? SpeedKmh,
    bool IsValid,
    string? RejectionReason
);

public record PositionHistoryResponse(
    Guid Id,
    Guid VehicleId,
    DateTime RecordedAt,
    double Lat,
    double Lon,
    decimal? SpeedKmh,
    short? HeadingDeg
);