using App.Domain.Materials.Entities;

namespace App.Interfaces.Ports.Materials;

public interface IMaterialRepository
{
    Task<List<TMaterial>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TMaterial>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<TMaterial?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);

    void Add(TMaterial material);
    void Update(TMaterial material);
    void Remove(TMaterial material);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}