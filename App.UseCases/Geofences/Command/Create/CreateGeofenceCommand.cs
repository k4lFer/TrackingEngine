using App.Objects.Geofences.DTOs.Input.Command;
using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Geofences.Command.Create;

public class CreateGeofenceCommand : ICommand<OutputPort<GeofenceResponse>>
{
    public CreateGeofenceRequest Input { get; }

    public CreateGeofenceCommand(CreateGeofenceRequest input)
    {
        Input = input;
    }
}