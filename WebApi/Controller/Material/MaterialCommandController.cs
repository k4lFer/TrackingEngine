using App.Objects.Materials.DTOs.Input.Command;
using App.UseCases.Materials.Command.Create;
using App.UseCases.Materials.Command.Delete;
using App.UseCases.Materials.Command.Update;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Material;

[ApiController]
[Route("api/materials")]
[Tags("Materiales")]
[Produces("application/json")]
public class MaterialCommandController : ControllerBase
{
    private readonly IMediator _mediator;

    public MaterialCommandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Crear material")]
    [EndpointDescription("Registra un nuevo material en el sistema")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateMaterialRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateMaterialCommand(request);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Actualizar material")]
    [EndpointDescription("Actualiza los datos de un material existente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMaterialRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateMaterialCommand(id, request);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Eliminar material")]
    [EndpointDescription("Elimina un material del sistema")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteMaterialCommand(id);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}