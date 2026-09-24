using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Query;

namespace App.Interfaces.Ports.Roads;

public interface IRoadQueryRepository
{
    Task<QueryResult<RoadResponse>> GetRoadsPagedAsync(
        int page,
        int pageSize,
        QueryFilter<RoadResponse>? filter = null,
        CancellationToken cancellationToken = default);
}