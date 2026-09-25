using System.Security.Claims;
using App.Domain.User.Entities;
using App.Interfaces.Ports;
using App.Interfaces.Ports.Auth;
using App.Interfaces.Ports.User;
using App.Objects.User.DTOs.Input.Command;
using App.Objects.User.DTOs.Output.Response;
using App.Shared.Common.Enums;
using App.Shared.Common.Result;
using App.Shared.Common.Security;
using App.Shared.Common.Validation;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, OutputPort<LoginResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenProvider _tokenProvider;
    private readonly ITokenHasher _tokenHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenCookieService _tokenCookieService;
    private readonly IInputValidator<LoginDto> _validator;
    
    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenProvider tokenProvider,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork,
        ITokenCookieService tokenCookieService,
        IInputValidator<LoginDto> validator)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenProvider = tokenProvider;
        _tokenHasher = tokenHasher;
        _unitOfWork = unitOfWork;
        _tokenCookieService = tokenCookieService;
        _validator = validator;
    }
    
    public async Task<OutputPort<LoginResponseDto>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        if (!await _validator.ValidateAsync(command.Input, cancellationToken))
        {
            return OutputPort<LoginResponseDto>.Failure(_validator.StatusCode, _validator.Messages.ToArray());
        }
        
        var dto = command.Input;
        
        var user = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        
        if (user is null || !_passwordHasher.VerifyHashedPassword(user.Password, dto.Password))
        {
            return OutputPort<LoginResponseDto>.Failure(
                System.Net.HttpStatusCode.Unauthorized,
                new MessageDto(code: "INVALID_CREDENTIALS", message: "Invalid email or password.")
            );
        }

        if (!user.IsEmailVerified)
        {
            return OutputPort<LoginResponseDto>.Failure(
                System.Net.HttpStatusCode.Forbidden,
                new MessageDto(code: "EMAIL_NOT_VERIFIED", message: "You must verify your email before signing in.")
            );
        }
        
        await _refreshTokenRepository.RevokeExpiredUserTokensAsync(user.Id, DateTime.UtcNow, _tokenProvider.RefreshInactivityWindow(), cancellationToken);

        var deviceId = dto.DeviceId ?? Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };
        
        var accessToken = _tokenProvider.GenerateToken(user.Id.ToString(), claims, TokenType.Access);
        var refreshToken = _tokenProvider.GenerateToken(user.Id.ToString(), claims, TokenType.Refresh);
        var refreshTokenHash = _tokenHasher.Hash(refreshToken);
        
        var refreshTokenEntity = TRefreshToken.Create(
            user.Id,
            deviceId,
            refreshTokenHash,
            expiresAt: _tokenProvider.GetExpiration(TokenType.Refresh),
            command.IpAddress,
            command.UserAgent
        );
        
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);
        
        var response = new LoginResponseDto
        {
            Token = accessToken,
            //RefreshToken = refreshToken,
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            DeviceId = deviceId
        };

        _tokenCookieService.SetTokenCookie(refreshToken);
        
        return OutputPort<LoginResponseDto>.Success(response, System.Net.HttpStatusCode.OK, "Login successful");
    }
}
