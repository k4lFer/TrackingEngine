using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Roads.Command.Delete;

public class DeleteRoadCommand : ICommand<OutputPort<RoadResponse>>
{
    public Guid Id { get; }

    public DeleteRoadCommand(Guid id)
    {
        Id = id;
    }
}