using App.Objects.User.DTOs.Input.Command;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.VerifyEmail;

public class VerifyEmailCommand : ICommand<OutputPort<object>>
{
    public VerifyEmailDto Input { get; }

    public VerifyEmailCommand(VerifyEmailDto input)
    {
        Input = input;
    }
}