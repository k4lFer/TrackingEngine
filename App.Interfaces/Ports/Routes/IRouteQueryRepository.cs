using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Query;

namespace App.Interfaces.Ports.Routes;

public interface IRouteQueryRepository
{
    Task<QueryResult<RouteResponse>> GetRoutesPagedAsync(
        int page,
        int pageSize,
        QueryFilter<RouteResponse>? filter = null,
        bool onlyActive = true,
        CancellationToken cancellationToken = default);
}