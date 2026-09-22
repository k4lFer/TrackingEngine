using App.Objects.Shared.DTOs;

namespace App.Objects.Roads.DTOs.Input.Command;

public class CreateRoadRequest
{
    public string Code { get; set; }
    public string Name { get; set; }
    public List<CoordinateDto>? Waypoints { get; set; }
    public int? MaxSpeedKmh { get; set; }
}