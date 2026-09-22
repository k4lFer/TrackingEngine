using App.UseCases.Tracking.Query.GetAllTrips;
using App.UseCases.Tracking.Query.GetByIdTrip;
using App.UseCases.Tracking.Query.GetTripsByVehicle;
using App.UseCases.Tracking.Query.GetTripTrack;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Tracking;

[ApiController]
[Route("api/trips")]
[Tags("Viajes / Operaciones")]
[Produces("application/json")]
public class TripQueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public TripQueryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Listar viajes")]
    [EndpointDescription("Obtiene todos los viajes (ida cargada y retorno) con sus datos de resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllTripsQuery();
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("vehicle/{vehicleId:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Viajes de un vehículo")]
    [EndpointDescription("Obtiene los viajes realizados por un vehículo específico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByVehicle([FromRoute] Guid vehicleId, CancellationToken cancellationToken)
    {
        var query = new GetTripsByVehicleQuery(vehicleId);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Obtener viaje")]
    [EndpointDescription("Obtiene el detalle completo de un viaje incluyendo su trazado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetByIdTripQuery(id);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpGet("{id:guid}/track")]
    [AllowAnonymous]
    [EndpointSummary("Trazado de un viaje")]
    [EndpointDescription("Obtiene la secuencia de posiciones GPS registradas durante un viaje")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrack([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTripTrackQuery(id);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}