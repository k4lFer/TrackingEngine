using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Tracking.Command.CancelTrip;

public class CancelTripCommand : ICommand<OutputPort<TripSummaryResponse>>
{
    public Guid Id { get; }

    public CancelTripCommand(Guid id)
    {
        Id = id;
    }
}