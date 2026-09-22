using App.Objects.User.DTOs.Output.Response;
using App.Shared.Query;

namespace App.Interfaces.Ports.User;

public interface IRefreshTokenQueryRepository
{
    Task<QueryResult<ActiveSessionDto>> GetActiveSessionsAsync(Guid userId, int page, int pageSize, string? currentDeviceId = null, QueryFilter<ActiveSessionDto>? filter = null, CancellationToken cancellationToken = default);
}