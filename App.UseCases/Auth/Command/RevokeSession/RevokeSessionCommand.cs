using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.RevokeSession;

public class RevokeSessionCommand : ICommand<OutputPort<object>>
{
    public Guid UserId { get; }
    public Guid SessionId { get; }

    public RevokeSessionCommand(Guid userId, Guid sessionId)
    {
        UserId = userId;
        SessionId = sessionId;
    }
}