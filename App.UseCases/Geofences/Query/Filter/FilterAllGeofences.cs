using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Common.Query;

namespace App.UseCases.Geofences.Query.Filter;

public class FilterAllGeofences : QueryFilter<GeofenceResponse>
{
    public string? Search { get; set; }

    public bool? Active { get; set; }

    public override IQueryable<GeofenceResponse> ApplyFilter(IQueryable<GeofenceResponse> query)
    {
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            query = query.Where(g =>
                g.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                g.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (Active.HasValue)
        {
            query = query.Where(g => g.Active == Active.Value);
        }

        return query;
    }
}