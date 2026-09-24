namespace App.Objects.Vehicles.DTOs.Output.Response;

public record VehicleResponse(
    Guid Id,
    string Code,
    string Plate,
    string Brand,
    string Model,
    bool Active,
    string? DeviceIdentifier,
    int? DeviceKind,
    string? DeviceModel
);