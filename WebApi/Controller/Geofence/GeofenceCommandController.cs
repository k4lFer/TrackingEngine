using App.Objects.Geofences.DTOs.Input.Command;
using App.UseCases.Geofences.Command.Create;
using App.UseCases.Geofences.Command.Delete;
using App.UseCases.Geofences.Command.Update;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Geofence;

[ApiController]
[Route("api/geofences")]
[Tags("Geofences")]
[Produces("application/json")]
public class GeofenceCommandController : ControllerBase
{
    private readonly IMediator _mediator;

    public GeofenceCommandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Crear geofence")]
    [EndpointDescription("Registra un nuevo geofence en el sistema")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateGeofenceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateGeofenceCommand(request);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Actualizar geofence")]
    [EndpointDescription("Actualiza los datos de un geofence existente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateGeofenceRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateGeofenceCommand(id, request);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Eliminar geofence")]
    [EndpointDescription("Elimina un geofence del sistema")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteGeofenceCommand(id);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}