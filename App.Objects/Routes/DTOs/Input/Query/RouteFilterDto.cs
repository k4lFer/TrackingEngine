using App.Shared.Common.Query;

namespace App.Objects.Routes.DTOs.Input.Query;

public class RouteFilterDto : QueryDto
{
    public RouteFilterDto() : base(pageSize: 500)
    {
    }

    public string? Search { get; set; }

    public bool? Active { get; set; }
}