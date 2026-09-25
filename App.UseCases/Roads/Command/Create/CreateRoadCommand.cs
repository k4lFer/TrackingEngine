using App.Objects.Roads.DTOs.Input.Command;
using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Roads.Command.Create;

public class CreateRoadCommand : ICommand<OutputPort<RoadResponse>>
{
    public CreateRoadRequest Input { get; }

    public CreateRoadCommand(CreateRoadRequest input)
    {
        Input = input;
    }
}