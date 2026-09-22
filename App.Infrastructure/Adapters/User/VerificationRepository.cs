using App.Domain.User.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.User;
using App.Shared.Objects.Enums;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.User;

public class VerificationRepository : BaseRepository<TEmailVerification>, IVerificationRepository
{
    public VerificationRepository(AppDataBaseContext dbc) : base(dbc) { }

    public async Task<TEmailVerification?> GetActiveByTokenHashAsync(
        string tokenHash,
        VerificationType type,
        CancellationToken cancellationToken = default)
    {
        return await _dbc.EmailVerifications
            .FirstOrDefaultAsync(v =>
                v.TokenHash == tokenHash &&
                v.Type == type &&
                v.IsValid &&
                v.UsedAt == null,
            cancellationToken);
    }

    public async Task<TEmailVerification?> GetActiveByUserIdAsync(
        Guid userId,
        VerificationType type,
        CancellationToken cancellationToken = default)
    {
        return await _dbc.EmailVerifications
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync(v =>
                v.UserId == userId &&
                v.Type == type &&
                v.IsValid &&
                v.UsedAt == null,
            cancellationToken);
    }
}