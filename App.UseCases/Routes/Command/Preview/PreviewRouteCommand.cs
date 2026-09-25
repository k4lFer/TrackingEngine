using App.Objects.Routes.DTOs.Input.Command;
using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Routes.Command.Preview;

public class PreviewRouteCommand : ICommand<OutputPort<RouteResponse>>
{
    public RoutePreviewRequest Input { get; }

    public PreviewRouteCommand(RoutePreviewRequest input)
    {
        Input = input;
    }
}