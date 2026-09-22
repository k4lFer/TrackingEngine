using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Roads.Query.GetById;

public class GetByIdRoadQuery : IQuery<OutputPort<RoadResponse>>
{
    public Guid Id { get; }

    public GetByIdRoadQuery(Guid id) => Id = id;
}