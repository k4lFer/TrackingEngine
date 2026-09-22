using System.Net;
using App.Interfaces.Ports.Routes;
using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Routes.Query.GetById;

public class GetByIdRouteQueryHandler : IQueryHandler<GetByIdRouteQuery, OutputPort<RouteResponse>>
{
    private readonly IRouteRepository _routeRepository;

    public GetByIdRouteQueryHandler(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<OutputPort<RouteResponse>> Handle(GetByIdRouteQuery query, CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetByIdAsync(query.Id, cancellationToken);
        if (route is null)
        {
            return OutputPort<RouteResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró la ruta.", "ROUTE_NOT_FOUND"));
        }

        return OutputPort<RouteResponse>.Success(data: route);
    }
}