using App.Domain.User.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.User;
using App.Objects.User.DTOs.Output.Response;
using App.Shared.Objects.Enums;
using App.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.User;

public class RefreshTokenQueryRepository : BaseRepository<TRefreshToken>, IRefreshTokenQueryRepository
{
    public RefreshTokenQueryRepository(AppDataBaseContext dbc) : base(dbc) { }

    public async Task<QueryResult<ActiveSessionDto>> GetActiveSessionsAsync(
        Guid userId, int page, int pageSize, string? currentDeviceId = null,
        QueryFilter<ActiveSessionDto>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbc.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .Select(rt => new ActiveSessionDto
            {
                Id = rt.Id,
                DeviceId = rt.DeviceId,
                IpAddress = rt.IpAddress,
                UserAgent = rt.UserAgent,
                CreatedAt = rt.CreatedAt,
                LastActivityAt = rt.LastActivityAt,
                ExpiresAt = rt.ExpiresAt,
                RevokedAt = rt.RevokedAt,
                IsCurrentDevice = rt.DeviceId == currentDeviceId
            });

        if (filter is not null)
            query = filter.ApplyFilter(query);

        return await PaginateAsync(query, page, pageSize, cancellationToken);
    }
}