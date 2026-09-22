using System.Net;
using App.Interfaces.Ports.Routes;
using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Routes.Query.GetAll;

public class GetAllRouteQueryHandler : IQueryHandler<GetAllRouteQuery, OutputPort<QueryResult<RouteResponse>>>
{
    private readonly IRouteRepository _routeRepository;

    public GetAllRouteQueryHandler(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<OutputPort<QueryResult<RouteResponse>>> Handle(GetAllRouteQuery query, CancellationToken cancellationToken)
    {
        var results = await _routeRepository.GetAllActiveAsync(cancellationToken);

        return OutputPort<QueryResult<RouteResponse>>.Success(
            data: QueryResult<RouteResponse>.Success(results, results.Count, 1, 1, results.Count));
    }
}