using App.Shared.Common.Query;

namespace App.Objects.Geofences.DTOs.Input.Query;

public class GeofenceFilterDto : QueryDto
{
    public GeofenceFilterDto() : base(pageSize: 500)
    {
    }

    public string? Search { get; set; }

    public bool? Active { get; set; }
}