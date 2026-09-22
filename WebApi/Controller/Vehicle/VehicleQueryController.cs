using App.UseCases.Vehicles.Query.GetAll;
using App.UseCases.Vehicles.Query.GetById;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Vehicle;

[ApiController]
[Route("api/vehicles")]
[Tags("Vehículos")]
[Produces("application/json")]
public class VehicleQueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehicleQueryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Listar vehículos")]
    [EndpointDescription("Obtiene todos los vehículos junto con su estado en vivo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllVehicleQuery();
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("status")]
    [AllowAnonymous]
    [EndpointSummary("Estado en vivo de los vehículos")]
    [EndpointDescription("Obtiene el estado actual de todos los vehículos (posicion, viaje activo)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllWithStatus(CancellationToken cancellationToken)
    {
        var query = new GetAllVehicleQuery();
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Obtener vehículo por ID")]
    [EndpointDescription("Obtiene un vehículo por su ID incluyendo su estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetByIdVehicleQuery(id);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}