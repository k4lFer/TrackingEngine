using App.Objects.User.DTOs.Input.Query;
using App.Shared.Common.Security;
using App.UseCases.Auth.Query.GetActiveSessions;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Auth;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
[Produces("application/json")]
public class AuthQueryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public AuthQueryController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("sessions")]
    [Authorize]
    [EndpointSummary("Sesiones del usuario")]
    [EndpointDescription("Devuelve las sesiones del usuario actual. Por defecto solo las activas. Usa el parámetro Status: Active (1), Revoked (2) o All (3).")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveSessions(
        [FromQuery] ActiveSessionFilterDto filter,
        CancellationToken cancellationToken)
    {
        var userClaims = _currentUser.GetClaim();
        if (userClaims is null) return Unauthorized();

        var currentDeviceId = HttpContext.Request.Headers["X-Device-Id"].FirstOrDefault();
        var query = new GetActiveSessionsQuery(userClaims.Id, currentDeviceId, filter);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);

        return ResponseHelper.GetActionResult(result);
    }
}