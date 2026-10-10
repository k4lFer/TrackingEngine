namespace App.Objects.Vehicles.DTOs.Output.Response;

public sealed record VehicleIngestSnapshot(
    Guid Id,
    string Code,
    Guid? ActiveTripId);
