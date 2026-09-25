using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetLatestPositions;

public class GetLatestPositionsQuery : IQuery<OutputPort<List<PositionResponse>>>
{
}