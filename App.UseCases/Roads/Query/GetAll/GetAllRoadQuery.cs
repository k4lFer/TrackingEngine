using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Roads.Query.GetAll;

public class GetAllRoadQuery : IQuery<OutputPort<QueryResult<RoadResponse>>>
{
}