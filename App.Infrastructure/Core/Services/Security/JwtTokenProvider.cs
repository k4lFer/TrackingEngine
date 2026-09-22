using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using App.Shared.Objects.Enums;
using App.Shared.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace App.Infrastructure.Core.Services.Security;

public class JwtTokenProvider : ITokenProvider
{
    private readonly IConfiguration _configuration;

    public JwtTokenProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private static readonly Dictionary<TokenType, string> TokenTypeKeys = new()
    {
        { TokenType.Access, "Jwt:AccessTokenKey" },
        { TokenType.Refresh, "Jwt:RefreshTokenKey" },
        { TokenType.PasswordReset, "Jwt:PasswordResetKey" },
        { TokenType.EmailConfirmation, "Jwt:EmailConfirmationKey" }
    };

    private string GetKey(TokenType type)
    {
        if (!TokenTypeKeys.TryGetValue(type, out var configKey))
            throw new ArgumentException($"TokenType inválido: {type}");

        var key = _configuration[configKey];

        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException($"No se encontró la llave JWT en configuración: {configKey}");

        return key;
    }

    private SymmetricSecurityKey GetSecurityKey(TokenType type)
    {
        var key = GetKey(type);
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    public DateTime GetExpiration(TokenType type)
    {
        return type switch
        {
            TokenType.Access => DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60")
            ),

            TokenType.Refresh => DateTime.UtcNow.AddDays(
                double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7")
            ),

            TokenType.PasswordReset => DateTime.UtcNow.AddHours(
                double.Parse(_configuration["Jwt:PasswordResetExpirationHours"] ?? "24")
            ),

            TokenType.EmailConfirmation => DateTime.UtcNow.AddHours(
                double.Parse(_configuration["Jwt:EmailConfirmationExpirationHours"] ?? "24")
            ),

            _ => DateTime.UtcNow.AddMinutes(60)
        };
    }

    public TimeSpan RefreshInactivityWindow()
    {
        return TimeSpan.FromDays(
            double.Parse(_configuration["Jwt:RefreshTokenInactivityDays"] ?? "30")
        );
    }

    public string GenerateToken(string subject, IEnumerable<Claim> claims, TokenType type)
    {
        var securityKey = GetSecurityKey(type);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var allClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, subject)
        };

        allClaims.AddRange(claims);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(allClaims),
            Expires = GetExpiration(type),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public bool ValidateToken(string token, TokenType type)
    {
        var securityKey = GetSecurityKey(type);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = securityKey,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],

            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string ExtractSubject(string token, TokenType type)
    {
        return ExtractClaim(token, ClaimTypes.NameIdentifier, type)?.ToString() ?? string.Empty;
    }

    public object? ExtractClaim(string token, string claimKey, TokenType type)
    {
        var securityKey = GetSecurityKey(type);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = securityKey,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

            return principal.FindFirst(claimKey)?.Value;
        }
        catch
        {
            return null;
        }
    }
}