using App.Objects.User.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.RefreshToken;

public class RefreshTokenCommand : ICommand<OutputPort<LoginResponseDto>>
{
}