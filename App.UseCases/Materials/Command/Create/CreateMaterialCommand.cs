using App.Objects.Materials.DTOs.Input.Command;
using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Materials.Command.Create;

public class CreateMaterialCommand : ICommand<OutputPort<MaterialResponse>>
{
    public CreateMaterialRequest Input { get; }

    public CreateMaterialCommand(CreateMaterialRequest input)
    {
        Input = input;
    }
}