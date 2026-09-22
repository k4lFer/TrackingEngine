using App.Interfaces.Ports.Auth;
using Microsoft.AspNetCore.Http;

namespace App.Infrastructure.Core.Services.Auth;

public class TokenCookieService : ITokenCookieService
{
    private const string CookieName = "refresh_token";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenCookieService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetTokenCookie()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[CookieName];
    }

    public void RemoveTokenCookie()
    {
        var context = _httpContextAccessor.HttpContext!;
        context.Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            Path = "/"
        });
    }

    public void SetTokenCookie(string token)
    {
        var context = _httpContextAccessor.HttpContext!;
        context.Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = "/"
        });
    }
}