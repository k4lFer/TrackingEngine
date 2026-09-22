using App.Objects.Vehicles.DTOs.Input.Command;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Vehicles.Command.Create;

public class CreateVehicleCommand : ICommand<OutputPort<VehicleResponse>>
{
    public CreateVehicleRequest Input { get; }

    public CreateVehicleCommand(CreateVehicleRequest input)
    {
        Input = input;
    }
}