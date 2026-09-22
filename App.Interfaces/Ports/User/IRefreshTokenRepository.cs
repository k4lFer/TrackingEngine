using App.Domain.User.Entities;

namespace App.Interfaces.Ports.User;

public interface IRefreshTokenRepository : IBaseRepository<TRefreshToken>
{
    Task<TRefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, string? excludeDeviceId = null, CancellationToken cancellationToken = default);
    Task RevokeTokensByDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);
    Task RevokeExpiredUserTokensAsync(Guid userId, DateTime now, TimeSpan inactivityWindow, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(Guid tokenId, CancellationToken cancellationToken = default);
}