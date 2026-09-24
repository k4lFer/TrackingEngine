using App.Objects.Materials.DTOs.Input.Query;
using App.UseCases.Materials.Query.GetAll;
using App.UseCases.Materials.Query.GetById;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Material;

[ApiController]
[Route("api/materials")]
[Tags("Materiales")]
[Produces("application/json")]
public class MaterialQueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public MaterialQueryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Listar materiales")]
    [EndpointDescription("Obtiene todos los materiales registrados. Admite paginación (NumberPage/PageSize) y filtros (Search, Active).")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] MaterialFilterDto filter, CancellationToken cancellationToken)
    {
        var query = new GetAllMaterialQuery(filter);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Obtener material por ID")]
    [EndpointDescription("Obtiene un material por su ID")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetByIdMaterialQuery(id);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}