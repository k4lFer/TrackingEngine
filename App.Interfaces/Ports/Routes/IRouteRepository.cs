using App.Domain.Routes.Entities;
using App.Objects.Routes.DTOs.Output.Response;

namespace App.Interfaces.Ports.Routes;

public interface IRouteRepository
{
    Task<IReadOnlyList<RouteResponse>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<RouteResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TRoute?> GetByIdRawAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TRoute>> GetGroupRawAsync(Guid routeGroupId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);

    void Add(TRoute route);
    void Remove(TRoute route);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}