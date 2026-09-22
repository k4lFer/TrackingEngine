using App.Objects.User.DTOs.Input.Command;
using App.Shared.Security;
using App.UseCases.Auth.Command.Login;
using App.UseCases.Auth.Command.Logout;
using App.UseCases.Auth.Command.RefreshToken;
using App.UseCases.Auth.Command.ResendVerification;
using App.UseCases.Auth.Command.RevokeSession;
using App.UseCases.Auth.Command.RevokeAllSessions;
using App.UseCases.Auth.Command.SignUp;
using App.UseCases.Auth.Command.VerifyEmail;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helper;

namespace WebApi.Controller.Auth;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
[Produces("application/json")]
public class AuthCommandController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public AuthCommandController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpPost("sign-up")]
    [AllowAnonymous]
    [EndpointSummary("Registrar usuario")]
    [EndpointDescription("Permite a un usuario registrarse en el sistema")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SignUp([FromBody] SignUpDto dto, CancellationToken cancellationToken)
    {
        var command = new SignUpCommand(dto);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [EndpointSummary("Verificar correo electrónico")]
    [EndpointDescription("Confirma la cuenta usando el token del enlace o el código OTP recibido por email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto, CancellationToken cancellationToken)
    {
        var command = new VerifyEmailCommand(dto);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    [EndpointSummary("Reenviar correo de verificación")]
    [EndpointDescription("Genera un nuevo token y código OTP, invalida los anteriores y reenvía el email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto dto, CancellationToken cancellationToken)
    {
        var command = new ResendVerificationCommand(dto);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("Iniciar sesión")]
    [EndpointDescription("Permite a un usuario iniciar sesión en el sistema")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
        var command = new LoginCommand(dto, ipAddress, userAgent);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EndpointSummary("Refrescar token")]
    [EndpointDescription("Permite a un usuario refrescar su token de acceso utilizando un token de refresco válido")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand();
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpPost("logout")]
    [Authorize]
    [EndpointSummary("Cerrar sesión (dispositivo actual)")]
    [EndpointDescription("Revoca únicamente la sesión del dispositivo actual (identificado por el header X-Device-Id)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userClaims = _currentUser.GetClaim();
        if (userClaims is null) return Unauthorized();
        var deviceId = HttpContext.Request.Headers["X-Device-Id"].FirstOrDefault();
        var command = new LogoutCommand(userClaims.Id, deviceId);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpDelete("sessions/{sessionId:guid}")]
    [Authorize]
    [EndpointSummary("Revocar sesión específica")]
    [EndpointDescription("Revoca una sesión específica por su ID")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession([FromRoute] Guid sessionId, CancellationToken cancellationToken)
    {
        var userClaims = _currentUser.GetClaim();
        if (userClaims is null) return Unauthorized();
        var command = new RevokeSessionCommand(userClaims.Id, sessionId);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }

    [HttpDelete("sessions")]
    [Authorize]
    [EndpointSummary("Cerrar todas las sesiones excepto la actual")]
    [EndpointDescription("Revoca todos los refresh tokens activos del usuario excepto el del dispositivo actual (enviado en el header X-Device-Id)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeAllSessions(CancellationToken cancellationToken)
    {
        var userClaims = _currentUser.GetClaim();
        if (userClaims is null) return Unauthorized();
        var deviceId = HttpContext.Request.Headers["X-Device-Id"].FirstOrDefault();
        var command = new RevokeAllSessionsCommand(userClaims.Id, deviceId);
        var result = await _mediator.SendCommandAsync(command, cancellationToken);
        return ResponseHelper.GetActionResult(result);
    }
}
