using App.Objects.Tracking.DTOs.Input.Command;
using App.UseCases.Tracking.Command.CancelTrip;
using App.UseCases.Tracking.Command.CreateTrip;
using App.UseCases.Tracking.Command.UpdateTrip;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Tracking;

[ApiController]
[Route("api/trips")]
[Tags("Viajes / Operaciones")]
[Produces("application/json")]
public class TripCommandController : ControllerBase
{
    private readonly IMediator _mediator;

    public TripCommandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Iniciar viaje")]
    [EndpointDescription("Crea un viaje de forma manual para un vehículo. El vehículo debe estar activo y sin un viaje en curso.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateTripRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTripCommand(request);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Editar viaje en curso")]
    [EndpointDescription("Actualiza el material y/o la carga de un viaje que todavía está en curso.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTripRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTripCommand(id, request);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [AllowAnonymous]
    [EndpointSummary("Cancelar viaje en curso")]
    [EndpointDescription("Cancela un viaje que todavía está en curso.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelTripCommand(id);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}