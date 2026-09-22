using App.UseCases.Roads.Query.GetAll;
using App.UseCases.Roads.Query.GetById;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Road;

[ApiController]
[Route("api/roads")]
[Tags("Caminos")]
[Produces("application/json")]
public class RoadQueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoadQueryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Listar caminos")]
    [EndpointDescription("Obtiene todos los caminos activos de la red vial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllRoadQuery();
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Obtener camino por ID")]
    [EndpointDescription("Obtiene un camino por su ID")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetByIdRoadQuery(id);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}