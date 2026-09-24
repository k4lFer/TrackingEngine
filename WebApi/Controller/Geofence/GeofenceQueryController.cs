using App.Objects.Geofences.DTOs.Input.Query;
using App.UseCases.Geofences.Query.CheckPoint;
using App.UseCases.Geofences.Query.GetAll;
using App.UseCases.Geofences.Query.GetById;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Geofence;

[ApiController]
[Route("api/geofences")]
[Tags("Geofences")]
[Produces("application/json")]
public class GeofenceQueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public GeofenceQueryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Listar geofences")]
    [EndpointDescription("Obtiene todos los geofences registrados. Admite paginación (NumberPage/PageSize) y filtros (Search, Active).")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] GeofenceFilterDto filter, CancellationToken cancellationToken)
    {
        var query = new GetAllGeofenceQuery(filter);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Obtener geofence por ID")]
    [EndpointDescription("Obtiene un geofence por su ID")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetByIdGeofenceQuery(id);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("check-point")]
    [AllowAnonymous]
    [EndpointSummary("Geofence que contiene un punto")]
    [EndpointDescription("Obtiene el geofence activo de mayor prioridad que contiene las coordenadas indicadas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckPoint([FromQuery] double lat, [FromQuery] double lon, CancellationToken cancellationToken)
    {
        var query = new CheckPointGeofenceQuery(lat, lon);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}