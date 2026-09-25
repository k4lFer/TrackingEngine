namespace App.Objects.Vehicles.DTOs.Output.Response;

public record VehicleStatusResponse(
    Guid Id,
    string Code,
    string Plate,
    string Brand,
    string Model,
    bool Active,
    string? DeviceIdentifier,
    int? DeviceKind,
    string? DeviceModel,
    string State,
    double? LastLat,
    double? LastLon,
    DateTime? LastReportedAt,
    Guid? ActiveTripId,
    DateTime? StoppedSince
);