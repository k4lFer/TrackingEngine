using System.Security.Claims;
using App.Domain.User.Entities;
using App.Interfaces.Ports;
using App.Interfaces.Ports.Auth;
using App.Interfaces.Ports.User;
using App.Objects.User.DTOs.Output.Response;
using App.Shared.Common.Enums;
using App.Shared.Common.Result;
using App.Shared.Common.Security;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.RefreshToken;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, OutputPort<LoginResponseDto>>
{
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenHasher _tokenHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenCookieService _tokenCookieService;

    public RefreshTokenCommandHandler(
        ITokenProvider tokenProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork,
        ITokenCookieService tokenCookieService)
    {
        _tokenProvider = tokenProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenHasher = tokenHasher;
        _unitOfWork = unitOfWork;
        _tokenCookieService = tokenCookieService;
    }

    public async Task<OutputPort<LoginResponseDto>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var rawToken = _tokenCookieService.GetTokenCookie();

        if (string.IsNullOrEmpty(rawToken))
        {
            return OutputPort<LoginResponseDto>.Failure(
                System.Net.HttpStatusCode.Unauthorized,
                new MessageDto(code: "INVALID_REFRESH_TOKEN", message: "The refresh token is invalid or has been revoked.")
            );
        }

        var tokenHash = _tokenHasher.Hash(rawToken);

        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken is null)
        {
            _tokenCookieService.RemoveTokenCookie();
            return OutputPort<LoginResponseDto>.Failure(
                System.Net.HttpStatusCode.Unauthorized,
                new MessageDto(code: "INVALID_REFRESH_TOKEN", message: "The refresh token is invalid or has been revoked.")
            );
        }

        var expiredByInactivity = storedToken.IsExpiredByInactivity(DateTime.UtcNow, _tokenProvider.RefreshInactivityWindow());

        if (!storedToken.IsActive)
        {
            if (storedToken.RevokedAt is null)
                storedToken.Revoke();
            await _unitOfWork.SaveChanges(cancellationToken);
            _tokenCookieService.RemoveTokenCookie();

            return OutputPort<LoginResponseDto>.Failure(
                System.Net.HttpStatusCode.Unauthorized,
                new MessageDto(code: "INVALID_REFRESH_TOKEN", message: "The refresh token is invalid or has been revoked.")
            );
        }

        if (expiredByInactivity)
        {
            storedToken.Revoke();
            await _unitOfWork.SaveChanges(cancellationToken);
            _tokenCookieService.RemoveTokenCookie();

            return OutputPort<LoginResponseDto>.Failure(
                System.Net.HttpStatusCode.Unauthorized,
                new MessageDto(code: "SESSION_EXPIRED_INACTIVITY", message: "Your session expired due to inactivity.")
            );
        }

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (user is null)
        {
            _tokenCookieService.RemoveTokenCookie();
            return OutputPort<LoginResponseDto>.Failure(
                System.Net.HttpStatusCode.Unauthorized,
                new MessageDto(code: "USER_NOT_FOUND", message: "User not found.")
            );
        }

        storedToken.Revoke();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var newAccessToken = _tokenProvider.GenerateToken(user.Id.ToString(), claims, TokenType.Access);
        var newRefreshToken = _tokenProvider.GenerateToken(user.Id.ToString(), claims, TokenType.Refresh);
        var newRefreshTokenHash = _tokenHasher.Hash(newRefreshToken);

        var newRefreshTokenEntity = TRefreshToken.Create(
            user.Id,
            storedToken.DeviceId,
            newRefreshTokenHash,
            expiresAt: _tokenProvider.GetExpiration(TokenType.Refresh),
            storedToken.IpAddress,
            storedToken.UserAgent
        );

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        _tokenCookieService.SetTokenCookie(newRefreshToken);

        var response = new LoginResponseDto
        {
            Token = newAccessToken,
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            DeviceId = storedToken.DeviceId
        };

        return OutputPort<LoginResponseDto>.Success(response, System.Net.HttpStatusCode.OK, "Token refreshed successfully");
    }
}