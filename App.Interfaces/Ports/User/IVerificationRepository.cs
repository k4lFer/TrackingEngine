using App.Domain.User.Entities;
using App.Shared.Objects.Enums;

namespace App.Interfaces.Ports.User;

public interface IVerificationRepository : IBaseRepository<TEmailVerification>
{
    Task<TEmailVerification?> GetActiveByTokenHashAsync(string tokenHash, VerificationType type, CancellationToken cancellationToken = default);
    Task<TEmailVerification?> GetActiveByUserIdAsync(Guid userId, VerificationType type, CancellationToken cancellationToken = default);
}