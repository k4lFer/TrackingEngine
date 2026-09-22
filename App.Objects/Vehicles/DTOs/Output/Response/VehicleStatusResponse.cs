namespace App.Objects.Vehicles.DTOs.Output.Response;

public record VehicleStatusResponse(
    Guid Id,
    string Code,
    string Plate,
    string State,
    double? LastLat,
    double? LastLon,
    DateTime? LastReportedAt,
    Guid? ActiveTripId
);