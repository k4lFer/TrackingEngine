using App.Objects.User.DTOs.Input.Command;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.SignUp;

public class SignUpCommand : ICommand<OutputPort<Guid>>
{
    public SignUpDto Input { get; }
    
    public SignUpCommand(SignUpDto input)
    {
        Input = input;
    }
}
