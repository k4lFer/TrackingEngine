using App.Objects.Vehicles.DTOs.Input.Command;
using App.Objects.Vehicles.DTOs.Output.Response;
using App.Interfaces.Ports;
using App.Interfaces.Ports.Vehicles;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Vehicles.Command.Update;

public class UpdateVehicleCommand : ICommand<OutputPort<VehicleResponse>>
{
    public Guid Id { get; }
    public UpdateVehicleRequest Input { get; }

    public UpdateVehicleCommand(Guid id, UpdateVehicleRequest input)
    {
        Id = id;
        Input = input;
    }
}
