using App.Objects.Tracking.DTOs.Input.Command;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Tracking.Command.CreateTrip;

public class CreateTripCommand : ICommand<OutputPort<TripSummaryResponse>>
{
    public CreateTripRequest Input { get; }

    public CreateTripCommand(CreateTripRequest input)
    {
        Input = input;
    }
}