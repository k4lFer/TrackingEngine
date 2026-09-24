using App.Interfaces.Ports.Routes;
using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using App.UseCases.Routes.Query.Filter;
using Cortex.Mediator.Queries;

namespace App.UseCases.Routes.Query.GetAll;

public class GetAllRouteQueryHandler : IQueryHandler<GetAllRouteQuery, OutputPort<QueryResult<RouteResponse>>>
{
    private readonly IRouteQueryRepository _routeQueryRepository;

    public GetAllRouteQueryHandler(IRouteQueryRepository routeQueryRepository)
    {
        _routeQueryRepository = routeQueryRepository;
    }

    public async Task<OutputPort<QueryResult<RouteResponse>>> Handle(GetAllRouteQuery query, CancellationToken cancellationToken)
    {
        var filter = new FilterAllRoutes
        {
            Search = query.Filter.Search,
            Active = query.Filter.Active,
        };

        var results = await _routeQueryRepository.GetRoutesPagedAsync(
            query.Filter.NumberPage,
            query.Filter.PageSize,
            filter,
            query.Filter.Active ?? true,
            cancellationToken);

        return OutputPort<QueryResult<RouteResponse>>.Success(data: results);
    }
}