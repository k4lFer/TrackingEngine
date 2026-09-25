using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.Logout;

public class LogoutCommand : ICommand<OutputPort<object>>
{
    public Guid UserId { get; }
    public string? DeviceId { get; }

    public LogoutCommand(Guid userId, string? deviceId)
    {
        UserId = userId;
        DeviceId = deviceId;
    }
}
