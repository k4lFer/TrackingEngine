using App.Objects.Shared.DTOs;

namespace App.Objects.Routes.DTOs.Input.Command;

public class UpdateRouteRequest
{
    public string Name { get; set; }
    public List<CoordinateDto> Waypoints { get; set; }
    public int ToleranceM { get; set; }
    public int? MaxSpeedKmh { get; set; }
    public int? AlternativesCount { get; set; }
    public Guid? OriginGeofenceId { get; set; }
    public Guid? DestinationGeofenceId { get; set; }
    public bool Active { get; set; }
}