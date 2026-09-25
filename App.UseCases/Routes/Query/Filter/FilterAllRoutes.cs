using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Common.Query;

namespace App.UseCases.Routes.Query.Filter;

public class FilterAllRoutes : QueryFilter<RouteResponse>
{
    public string? Search { get; set; }

    public bool? Active { get; set; }

    public override IQueryable<RouteResponse> ApplyFilter(IQueryable<RouteResponse> query)
    {
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            query = query.Where(r =>
                r.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (Active.HasValue)
        {
            query = query.Where(r => r.Active == Active.Value);
        }

        return query;
    }
}