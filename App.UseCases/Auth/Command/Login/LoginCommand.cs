using App.Objects.User.DTOs.Input.Command;
using App.Objects.User.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.Login;

public class LoginCommand : ICommand<OutputPort<LoginResponseDto>>
{
    public LoginDto Input { get; }
    public string? IpAddress { get; }
    public string? UserAgent { get; }

    public LoginCommand(LoginDto input, string? ipAddress = null, string? userAgent = null)
    {
        Input = input;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}
