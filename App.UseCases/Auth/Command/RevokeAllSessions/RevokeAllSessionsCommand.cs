using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.RevokeAllSessions;

public class RevokeAllSessionsCommand : ICommand<OutputPort<object>>
{
    public Guid UserId { get; }
    public string? DeviceId { get; }

    public RevokeAllSessionsCommand(Guid userId, string? deviceId = null)
    {
        UserId = userId;
        DeviceId = deviceId;
    }
}