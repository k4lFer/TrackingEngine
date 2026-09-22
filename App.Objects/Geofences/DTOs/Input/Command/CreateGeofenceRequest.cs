using App.Objects.Shared.DTOs;

namespace App.Objects.Geofences.DTOs.Input.Command;

public class CreateGeofenceRequest
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Kind { get; set; }
    public int Priority { get; set; }
    public List<CoordinateDto> Polygon { get; set; }
    public decimal? MaxSpeedKmh { get; set; }
    public string? Color { get; set; }
}