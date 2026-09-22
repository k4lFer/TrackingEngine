using App.Objects.Tracking.DTOs.Input.Command;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Tracking.Command.ReportPosition;

public class ReportPositionCommand : ICommand<OutputPort<PositionResponse>>
{
    public PositionReportRequest Input { get; }

    public ReportPositionCommand(PositionReportRequest input)
    {
        Input = input;
    }
}