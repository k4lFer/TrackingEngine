using App.Objects.Routes.DTOs.Input.Command;
using App.UseCases.Routes.Command.Create;
using App.UseCases.Routes.Command.Delete;
using App.UseCases.Routes.Command.Preview;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Route;

[ApiController]
[Route("api/routes")]
[Tags("Rutas")]
[Produces("application/json")]
public class RouteCommandController : ControllerBase
{
    private readonly IMediator _mediator;

    public RouteCommandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("preview")]
    [AllowAnonymous]
    [EndpointSummary("Vista previa de ruta")]
    [EndpointDescription("Enruta los waypoints sobre la red vial con su perfil de velocidad, sin persistir nada")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Preview([FromBody] RoutePreviewRequest request, CancellationToken cancellationToken)
    {
        var command = new PreviewRouteCommand(request);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Crear ruta")]
    [EndpointDescription("Registra una nueva ruta enrutada por la red vial de la mina")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateRouteRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRouteCommand(request);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Eliminar ruta")]
    [EndpointDescription("Elimina una ruta del sistema")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteRouteCommand(id);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}