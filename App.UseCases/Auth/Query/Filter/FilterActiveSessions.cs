using App.Objects.User.DTOs.Output.Response;
using App.Shared.Query;

namespace App.UseCases.Auth.Query.Filter;

public class FilterActiveSessions : QueryFilter<ActiveSessionDto>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public override IQueryable<ActiveSessionDto> ApplyFilter(IQueryable<ActiveSessionDto> query)
    {
        if (FromDate.HasValue)
            query = query.Where(s => s.CreatedAt >= FromDate.Value);

        if (ToDate.HasValue)
            query = query.Where(s => s.CreatedAt <= ToDate.Value);

        return query;
    }
}