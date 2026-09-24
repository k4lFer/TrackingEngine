using App.Shared.Query;

namespace App.Objects.Tracking.DTOs.Input.Query;

public class TripFilterDto : QueryDto
{
    public TripFilterDto() : base(pageSize: 500)
    {
    }

    public string? Search { get; set; }

    public string? Status { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}