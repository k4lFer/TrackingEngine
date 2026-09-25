using App.Interfaces.Ports.Tracking;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Tracking.Query.GetRecentEvents;

public class GetRecentEventsQueryHandler : IQueryHandler<GetRecentEventsQuery, OutputPort<RecentEventsResponse>>
{
    private readonly ITrackingReadRepository _read;

    public GetRecentEventsQueryHandler(ITrackingReadRepository read)
    {
        _read = read;
    }

    public async Task<OutputPort<RecentEventsResponse>> Handle(GetRecentEventsQuery query, CancellationToken cancellationToken)
    {
        var events = await _read.GetRecentEventsAsync(query.Count, cancellationToken);
        return OutputPort<RecentEventsResponse>.Success(data: events);
    }
}