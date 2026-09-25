using App.Objects.Routes.DTOs.Input.Command;
using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Routes.Command.Create;

public class CreateRouteCommand : ICommand<OutputPort<RouteResponse>>
{
    public CreateRouteRequest Input { get; }

    public CreateRouteCommand(CreateRouteRequest input)
    {
        Input = input;
    }
}