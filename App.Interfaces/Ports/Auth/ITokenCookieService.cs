namespace App.Interfaces.Ports.Auth;

public interface ITokenCookieService
{
    string? GetTokenCookie();
    void SetTokenCookie(string token);
    void RemoveTokenCookie();
}