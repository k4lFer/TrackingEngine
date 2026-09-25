using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Routes.Query.GetById;

public class GetByIdRouteQuery : IQuery<OutputPort<RouteResponse>>
{
    public Guid Id { get; }

    public GetByIdRouteQuery(Guid id) => Id = id;
}