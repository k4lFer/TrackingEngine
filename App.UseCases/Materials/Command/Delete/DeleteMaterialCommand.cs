using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Materials.Command.Delete;

public class DeleteMaterialCommand : ICommand<OutputPort<MaterialResponse>>
{
    public Guid Id { get; }

    public DeleteMaterialCommand(Guid id)
    {
        Id = id;
    }
}