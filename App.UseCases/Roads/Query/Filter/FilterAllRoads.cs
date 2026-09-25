using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Common.Query;

namespace App.UseCases.Roads.Query.Filter;

public class FilterAllRoads : QueryFilter<RoadResponse>
{
    public string? Search { get; set; }

    public override IQueryable<RoadResponse> ApplyFilter(IQueryable<RoadResponse> query)
    {
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            query = query.Where(r =>
                r.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }
}