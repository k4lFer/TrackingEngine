using App.Objects.Roads.DTOs.Input.Query;
using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Common.Query;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Roads.Query.GetAll;

public class GetAllRoadQuery : IQuery<OutputPort<QueryResult<RoadResponse>>>
{
    public RoadFilterDto Filter { get; }

    public GetAllRoadQuery(RoadFilterDto filter)
    {
        Filter = filter;
    }
}