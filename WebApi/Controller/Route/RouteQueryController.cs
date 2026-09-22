using App.UseCases.Routes.Query.GetAll;
using App.UseCases.Routes.Query.GetById;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Route;

[ApiController]
[Route("api/routes")]
[Tags("Rutas")]
[Produces("application/json")]
public class RouteQueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public RouteQueryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Listar rutas")]
    [EndpointDescription("Obtiene todas las rutas activas con sus geocercas de origen y destino")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllRouteQuery();
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Obtener ruta por ID")]
    [EndpointDescription("Obtiene una ruta por su ID incluyendo sus geocercas de origen y destino")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetByIdRouteQuery(id);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}