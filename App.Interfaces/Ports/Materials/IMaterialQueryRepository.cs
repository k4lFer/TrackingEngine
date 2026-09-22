using App.Domain.Materials.Entities;

namespace App.Interfaces.Ports.Materials;

public interface IMaterialQueryRepository
{
    Task<List<TMaterial>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}