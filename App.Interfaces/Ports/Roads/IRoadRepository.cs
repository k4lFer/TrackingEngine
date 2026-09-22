using App.Domain.Routes.Entities;

namespace App.Interfaces.Ports.Roads;

public interface IRoadRepository
{
    Task<List<TMineRoad>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TMineRoad>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<TMineRoad?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);

    void Add(TMineRoad road);
    void Remove(TMineRoad road);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}