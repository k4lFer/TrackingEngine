using App.Objects.Shared.DTOs;

namespace App.Objects.Geofences.DTOs.Input.Command;

public class UpdateGeofenceRequest
{
    public string Name { get; set; }
    public string Kind { get; set; }
    public int Priority { get; set; }
    public List<CoordinateDto> Polygon { get; set; }
    public decimal? MaxSpeedKmh { get; set; }
    public bool Active { get; set; }
}