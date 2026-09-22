using App.Objects.Shared.DTOs;

namespace App.Objects.Routes.DTOs.Input.Command;

public class RoutePreviewRequest
{
    public List<CoordinateDto> Waypoints { get; set; }
    public int ToleranceM { get; set; }
    public int? MaxSpeedKmh { get; set; }
}