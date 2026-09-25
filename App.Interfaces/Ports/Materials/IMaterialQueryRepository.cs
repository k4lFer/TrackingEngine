using App.Domain.Materials.Entities;
using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Common.Query;

namespace App.Interfaces.Ports.Materials;

public interface IMaterialQueryRepository
{
    Task<List<TMaterial>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    Task<QueryResult<MaterialResponse>> GetMaterialsPagedAsync(
        int page,
        int pageSize,
        QueryFilter<MaterialResponse>? filter = null,
        CancellationToken cancellationToken = default);
}