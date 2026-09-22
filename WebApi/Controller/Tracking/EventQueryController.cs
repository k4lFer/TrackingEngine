using App.UseCases.Tracking.Query.GetEventsByTrip;
using App.UseCases.Tracking.Query.GetEventsByVehicle;
using App.UseCases.Tracking.Query.GetRecentEvents;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Tracking;

[ApiController]
[Route("api/events")]
[Tags("Eventos")]
[Produces("application/json")]
public class EventQueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventQueryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Eventos recientes")]
    [EndpointDescription("Obtiene los eventos más recientes del motor (entrada/salida de zona, sobrevelocidad, desvío de ruta)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 50, CancellationToken cancellationToken = default)
    {
        var query = new GetRecentEventsQuery(count);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("vehicle/{vehicleId:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Eventos de un vehículo")]
    [EndpointDescription("Obtiene los eventos de un vehículo en un rango de fechas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByVehicle(
        [FromRoute] Guid vehicleId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var fromUtc = from ?? DateTime.UtcNow.AddHours(-24);
        var toUtc = to ?? DateTime.UtcNow;
        var query = new GetEventsByVehicleQuery(vehicleId, fromUtc, toUtc);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("trip/{tripId:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Eventos de un viaje")]
    [EndpointDescription("Obtiene los eventos generados durante un viaje específico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTrip([FromRoute] Guid tripId, CancellationToken cancellationToken)
    {
        var query = new GetEventsByTripQuery(tripId);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}