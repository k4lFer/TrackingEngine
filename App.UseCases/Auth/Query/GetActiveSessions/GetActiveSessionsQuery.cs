using App.Objects.User.DTOs.Input.Query;
using App.Objects.User.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Auth.Query.GetActiveSessions;

public class GetActiveSessionsQuery : IQuery<OutputPort<QueryResult<ActiveSessionDto>>>
{
    public Guid UserId { get; }
    public string? CurrentDeviceId { get; }
    public ActiveSessionFilterDto Filter { get; }

    public GetActiveSessionsQuery(Guid userId, string? currentDeviceId, ActiveSessionFilterDto filter)
    {
        UserId = userId;
        CurrentDeviceId = currentDeviceId;
        Filter = filter;
    }
}