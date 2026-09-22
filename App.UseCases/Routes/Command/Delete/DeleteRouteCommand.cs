using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Routes.Command.Delete;

public class DeleteRouteCommand : ICommand<OutputPort<RouteResponse>>
{
    public Guid Id { get; }

    public DeleteRouteCommand(Guid id)
    {
        Id = id;
    }
}