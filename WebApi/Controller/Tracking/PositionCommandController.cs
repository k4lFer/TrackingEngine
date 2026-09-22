using App.Objects.Tracking.DTOs.Input.Command;
using App.UseCases.Tracking.Command.ReportPosition;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Tracking;

[ApiController]
[Route("api/positions")]
[Tags("Posiciones")]
[Produces("application/json")]
public class PositionCommandController : ControllerBase
{
    private readonly IMediator _mediator;

    public PositionCommandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("report")]
    [AllowAnonymous]
    [EndpointSummary("Reportar posición")]
    [EndpointDescription("Registra una posición GPS del dispositivo de un vehículo y evalúa el motor (geocercas, sobrevelocidad, desvío de ruta)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Report([FromBody] PositionReportRequest request, CancellationToken cancellationToken)
    {
        var command = new ReportPositionCommand(request);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}