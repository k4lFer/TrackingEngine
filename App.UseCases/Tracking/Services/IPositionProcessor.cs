using App.Objects.Tracking.DTOs.Input.Command;

namespace App.UseCases.Tracking.Services;

public interface IPositionProcessor
{
    Task<PositionProcessOutcome> ProcessAsync(
        PositionReportRequest dto,
        CancellationToken cancellationToken);
}
