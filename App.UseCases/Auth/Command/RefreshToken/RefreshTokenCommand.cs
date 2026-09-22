using App.Objects.User.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.RefreshToken;

public class RefreshTokenCommand : ICommand<OutputPort<LoginResponseDto>>
{
}