using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Common.Query;

namespace App.Interfaces.Ports.Geofences;

public interface IGeofenceQueryRepository
{
    Task<QueryResult<GeofenceResponse>> GetGeofencesPagedAsync(
        int page,
        int pageSize,
        QueryFilter<GeofenceResponse>? filter = null,
        CancellationToken cancellationToken = default);
}