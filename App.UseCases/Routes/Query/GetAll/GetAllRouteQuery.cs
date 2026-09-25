using App.Objects.Routes.DTOs.Input.Query;
using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Common.Query;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Routes.Query.GetAll;

public class GetAllRouteQuery : IQuery<OutputPort<QueryResult<RouteResponse>>>
{
    public RouteFilterDto Filter { get; }

    public GetAllRouteQuery(RouteFilterDto filter)
    {
        Filter = filter;
    }
}