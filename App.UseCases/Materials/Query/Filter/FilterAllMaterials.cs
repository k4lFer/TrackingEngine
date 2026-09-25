using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Common.Query;

namespace App.UseCases.Materials.Query.Filter;

public class FilterAllMaterials : QueryFilter<MaterialResponse>
{
    public string? Search { get; set; }

    public bool? Active { get; set; }

    public override IQueryable<MaterialResponse> ApplyFilter(IQueryable<MaterialResponse> query)
    {
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            query = query.Where(m =>
                m.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (Active.HasValue)
        {
            query = query.Where(m => m.Active == Active.Value);
        }

        return query;
    }
}