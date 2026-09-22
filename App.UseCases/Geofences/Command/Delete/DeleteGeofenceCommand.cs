using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Geofences.Command.Delete;

public class DeleteGeofenceCommand : ICommand<OutputPort<GeofenceResponse>>
{
    public Guid Id { get; }

    public DeleteGeofenceCommand(Guid id)
    {
        Id = id;
    }
}