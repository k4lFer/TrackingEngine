using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Common.Query;

namespace App.UseCases.Tracking.Query.Filter;

public class FilterAllTrips : QueryFilter<TripSummaryResponse>
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public override IQueryable<TripSummaryResponse> ApplyFilter(IQueryable<TripSummaryResponse> query)
    {
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            query = query.Where(t =>
                t.VehicleCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (t.RouteCode != null && t.RouteCode.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (t.MaterialName != null && t.MaterialName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (t.OriginName != null && t.OriginName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (t.DestinationName != null && t.DestinationName.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(Status))
        {
            var statuses = Status
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            query = query.Where(t => statuses.Contains(t.Status));
        }

        if (FromDate.HasValue)
        {
            var from = FromDate.Value;
            query = query.Where(t => t.StartedAt >= from);
        }

        if (ToDate.HasValue)
        {
            var to = ToDate.Value;
            query = query.Where(t => t.StartedAt <= to);
        }

        return query;
    }
}