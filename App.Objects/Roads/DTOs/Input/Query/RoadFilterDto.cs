using App.Shared.Query;

namespace App.Objects.Roads.DTOs.Input.Query;

public class RoadFilterDto : QueryDto
{
    public RoadFilterDto() : base(pageSize: 500)
    {
    }

    public string? Search { get; set; }
}