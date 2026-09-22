using App.Objects.Vehicles.DTOs.Input.Command;
using App.UseCases.Vehicles.Command.Create;
using App.UseCases.Vehicles.Command.Delete;
using App.UseCases.Vehicles.Command.Update;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Vehicle;

[ApiController]
[Route("api/vehicles")]
[Tags("Vehículos")]
[Produces("application/json")]
public class VehicleCommandController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehicleCommandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Crear vehículo")]
    [EndpointDescription("Registra un nuevo vehículo en el sistema")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateVehicleCommand(request);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Actualizar vehículo")]
    [EndpointDescription("Actualiza los datos de un vehículo existente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateVehicleRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateVehicleCommand(id, request);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Eliminar vehículo")]
    [EndpointDescription("Elimina un vehículo del sistema")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteVehicleCommand(id);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}