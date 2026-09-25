using App.Objects.Routes.DTOs.Input.Command;
using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Routes.Command.Update;

public class UpdateRouteCommand : ICommand<OutputPort<RouteResponse>>
{
    public Guid Id { get; }
    public UpdateRouteRequest Input { get; }

    public UpdateRouteCommand(Guid id, UpdateRouteRequest input)
    {
        Id = id;
        Input = input;
    }
}