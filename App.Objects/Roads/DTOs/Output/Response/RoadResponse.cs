namespace App.Objects.Roads.DTOs.Output.Response;

public record RoadResponse(
    Guid Id,
    string Code,
    string Name,
    string GeoJsonLine,
    int? MaxSpeedKmh,
    bool Active,
    double DistanceKm
);