using App.Objects.Roads.DTOs.Input.Command;
using App.UseCases.Roads.Command.Create;
using App.UseCases.Roads.Command.Delete;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Road;

[ApiController]
[Route("api/roads")]
[Tags("Caminos")]
[Produces("application/json")]
public class RoadCommandController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoadCommandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Crear camino")]
    [EndpointDescription("Registra un nuevo camino en la red vial de la mina")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateRoadRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRoadCommand(request);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Eliminar camino")]
    [EndpointDescription("Elimina un camino de la red vial")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteRoadCommand(id);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}