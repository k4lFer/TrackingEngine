using App.Objects.Geofences.DTOs.Input.Command;
using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Geofences.Command.Update;

public class UpdateGeofenceCommand : ICommand<OutputPort<GeofenceResponse>>
{
    public Guid Id { get; }
    public UpdateGeofenceRequest Input { get; }

    public UpdateGeofenceCommand(Guid id, UpdateGeofenceRequest input)
    {
        Id = id;
        Input = input;
    }
}