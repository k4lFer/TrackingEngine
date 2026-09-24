using App.Objects.Tracking.DTOs.Input.Command;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Tracking.Command.UpdateTrip;

public class UpdateTripCommand : ICommand<OutputPort<TripSummaryResponse>>
{
    public Guid Id { get; }

    public UpdateTripRequest Input { get; }

    public UpdateTripCommand(Guid id, UpdateTripRequest input)
    {
        Id = id;
        Input = input;
    }
}