using App.Shared.Security;
using App.UseCases.User.Query.MyProfile;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.User;

[ApiController]
[Route("api/user")]
[Tags("Users")]
[Produces("application/json")]
public class UserQueryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public UserQueryController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [EndpointSummary("Obtener usuario por ID")]
    [EndpointDescription("Obtiene la información de un usuario por su ID")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserInfo([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return null;
    }

    [HttpGet("profile")]
    [AllowAnonymous]
    [EndpointSummary("Mi perfil")]
    [EndpointDescription("Obtiene el perfil del usuario autenticado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MyProfile(CancellationToken cancellationToken)
    {
        var userClaims = _currentUser.GetClaim();
        if (userClaims is null) return Unauthorized();
        var query = new MyProfileQuery(userClaims.Id);
        var result = await _mediator.SendQueryAsync(query, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}