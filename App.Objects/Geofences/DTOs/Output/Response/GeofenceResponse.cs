namespace App.Objects.Geofences.DTOs.Output.Response;

public record GeofenceResponse(
    Guid Id,
    string Code,
    string Name,
    string Kind,
    int Priority,
    string GeoJsonPolygon,
    decimal? MaxSpeedKmh,
    string Color,
    bool Active
);