using App.Domain.Materials.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Materials;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.Materials;

public class MaterialRepository : IMaterialRepository, IMaterialQueryRepository
{
    private readonly AppDataBaseContext _dbc;

    public MaterialRepository(AppDataBaseContext dbc)
    {
        _dbc = dbc;
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