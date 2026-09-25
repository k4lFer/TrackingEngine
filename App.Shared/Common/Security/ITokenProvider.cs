using System.Security.Claims;
using App.Shared.Common.Enums;


namespace App.Shared.Common.Security;

    public interface ITokenProvider
    {
        /// <summary>
        /// Genera un token firmado.
        /// </summary>
        /// <param name="subject">Identificador principal (userId, email, invitationId, etc.)</param>
        /// <param name="claims">Datos adicionales variables (roles, scope, expiración custom, etc.)</param>
        /// <param name="type">Tipo de token</param>
        /// <returns>Token generado</returns>
        string GenerateToken(string subject, IEnumerable<Claim> claims, TokenType type);

        /// <summary>
        /// Devuelve la fecha de expiración configurada para un tipo de token.
        /// </summary>
        DateTime GetExpiration(TokenType type);

        /// <summary>
        /// Devuelve la ventana de días configurada para expirar sesiones por inactividad (sliding).
        /// </summary>
        TimeSpan RefreshInactivityWindow();

        /// <summary>
        /// Valida si un token es correcto.
        /// </summary>
        bool ValidateToken(string token, TokenType type);

        /// <summary>
        /// Extrae el subject del token.
        /// </summary>
        string ExtractSubject(string token, TokenType type);

        /// <summary>
        /// Extrae un claim específico del token.
        /// </summary>
        object? ExtractClaim(string token, string claimKey, TokenType type);
    }