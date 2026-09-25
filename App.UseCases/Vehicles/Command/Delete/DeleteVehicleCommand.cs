using App.Objects.Vehicles.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Vehicles.Command.Delete;

public class DeleteVehicleCommand : ICommand<OutputPort<VehicleResponse>>
{
    public Guid Id { get; }

    public DeleteVehicleCommand(Guid id)
    {
        Id = id;
    }
}
