using App.Objects.User.DTOs.Input.Command;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.ResendVerification;

public class ResendVerificationCommand : ICommand<OutputPort<object>>
{
    public ResendVerificationDto Input { get; }

    public ResendVerificationCommand(ResendVerificationDto input)
    {
        Input = input;
    }
}