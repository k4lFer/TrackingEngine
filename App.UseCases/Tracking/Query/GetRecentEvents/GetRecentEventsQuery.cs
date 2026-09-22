using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetRecentEvents;

public class GetRecentEventsQuery : IQuery<OutputPort<RecentEventsResponse>>
{
    public int Count { get; }

    public GetRecentEventsQuery(int count) => Count = count;
}