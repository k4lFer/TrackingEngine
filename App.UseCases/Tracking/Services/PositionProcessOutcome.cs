using App.Shared.Common.Enums;

namespace App.UseCases.Tracking.Services;

public sealed record PositionProcessOutcome(
    bool Success,
    string? Error,
    Guid PositionId,
    Guid VehicleId,
    string? VehicleCode,
    Guid? ActiveTripId,
    DateTime RecordedAt,
    double Latitude,
    double Longitude,
    decimal? SpeedKmh,
    short? HeadingDeg,
    bool StateChanged,
    VehicleState State,
    bool IsBackfill);
