using App.Objects.Materials.DTOs.Input.Command;
using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Materials.Command.Update;

public class UpdateMaterialCommand : ICommand<OutputPort<MaterialResponse>>
{
    public Guid Id { get; }
    public UpdateMaterialRequest Input { get; }

    public UpdateMaterialCommand(Guid id, UpdateMaterialRequest input)
    {
        Id = id;
        Input = input;
    }
}