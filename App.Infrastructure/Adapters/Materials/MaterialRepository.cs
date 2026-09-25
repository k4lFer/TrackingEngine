using App.Domain.Materials.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Materials;
using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Common.Query;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.Materials;

public class MaterialRepository : BaseRepository<TMaterial>, IMaterialRepository, IMaterialQueryRepository
{
    public MaterialRepository(AppDataBaseContext dbc) : base(dbc)
    {
    }

    public async Task<List<TMaterial>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbc.Materials
            .AsNoTracking()
            .OrderBy(m => m.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TMaterial>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbc.Materials
            .AsNoTracking()
            .Where(m => m.Active)
            .OrderBy(m => m.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<QueryResult<MaterialResponse>> GetMaterialsPagedAsync(
        int page,
        int pageSize,
        QueryFilter<MaterialResponse>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var all = await _dbc.Materials
            .AsNoTracking()
            .OrderBy(m => m.Code)
            .Select(m => new MaterialResponse(m.Id, m.Code, m.Name, m.Unit, m.Active))
            .ToListAsync(cancellationToken);

        IQueryable<MaterialResponse> query = all.AsQueryable();
        if (filter is not null)
        {
            query = filter.ApplyFilter(query);
        }

        return PaginateInMemory(query.ToList(), page, pageSize);
    }

    public async Task<TMaterial?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbc.Materials
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbc.Materials
            .AnyAsync(m => m.Code == code, cancellationToken);
    }

    public void Add(TMaterial material)
    {
        _dbc.Materials.Add(material);
    }

    public void Update(TMaterial material)
    {
        _dbc.Materials.Update(material);
    }

    public void Remove(TMaterial material)
    {
        _dbc.Materials.Remove(material);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbc.SaveChangesAsync(cancellationToken);
    }
}