using App.UseCases.Tracking.Query.GetLatestPosition;
using App.UseCases.Tracking.Query.GetLatestPositions;
using App.UseCases.Tracking.Query.GetPositionHistory;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Tracking;

[ApiController]
[Route("api/positions")]
[Tags("Posiciones")]
[Produces("application/json")]
public class PositionQueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public PositionQueryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Últimas posiciones")]
    [EndpointDescription("Obtiene la última posición reportada por cada vehículo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllLatest(CancellationToken cancellationToken)
    {
        var query = new GetLatestPositionsQuery();
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("{vehicleId:guid}/latest")]
    [AllowAnonymous]
    [EndpointSummary("Última posición de un vehículo")]
    [EndpointDescription("Obtiene la última posición reportada por un vehículo específico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatest([FromRoute] Guid vehicleId, CancellationToken cancellationToken)
    {
        var query = new GetLatestPositionQuery(vehicleId);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("{vehicleId:guid}/history")]
    [AllowAnonymous]
    [EndpointSummary("Historial de posiciones")]
    [EndpointDescription("Obtiene el historial de posiciones de un vehículo en un rango de fechas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory(
        [FromRoute] Guid vehicleId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var fromUtc = from ?? DateTime.UtcNow.AddHours(-12);
        var toUtc = to ?? DateTime.UtcNow;
        var query = new GetPositionHistoryQuery(vehicleId, fromUtc, toUtc);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}