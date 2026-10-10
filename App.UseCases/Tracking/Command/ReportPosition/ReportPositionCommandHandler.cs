using App.Interfaces.Ports;
using App.Objects.Tracking.DTOs.Input.Command;
using App.Objects.Tracking.DTOs.Output.Response;
using App.Shared.Common.Result;
using App.UseCases.Tracking.Services;
using Cortex.Mediator.Commands;

namespace App.UseCases.Tracking.Command.ReportPosition;

/// <summary>
/// Handler HTTP/síncrono: delega el núcleo en <see cref="IPositionProcessor"/>
/// y persiste con un SaveChanges por trama. El camino batch (worker) reutiliza
/// el mismo procesador pero guarda por lote.
/// </summary>
public class ReportPositionCommandHandler : ICommandHandler<ReportPositionCommand, OutputPort<PositionResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPositionProcessor _processor;

    public ReportPositionCommandHandler(
        IUnitOfWork unitOfWork,
        IPositionProcessor processor)
    {
        _unitOfWork = unitOfWork;
        _processor = processor;
    }

    public async Task<OutputPort<PositionResponse>> Handle(ReportPositionCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Input;

        var outcome = await _processor.ProcessAsync(dto, cancellationToken);
        if (!outcome.Success)
        {
            return Rejected(dto, outcome.Error ?? "Posición rechazada");
        }

        await _unitOfWork.SaveChanges(cancellationToken);

        return OutputPort<PositionResponse>.Success(
            data: new PositionResponse(outcome.PositionId, outcome.VehicleId, outcome.RecordedAt,
                outcome.Latitude, outcome.Longitude, outcome.SpeedKmh, true, null),
            message: "Posición reportada correctamente.");
    }

    private static OutputPort<PositionResponse> Rejected(PositionReportRequest dto, string reason)
        => OutputPort<PositionResponse>.Success(
            data: new PositionResponse(Guid.Empty, dto.VehicleId, dto.RecordedAt,
                dto.Latitude, dto.Longitude, dto.SpeedKmh, false, reason),
            message: reason);
}
