using App.Domain.Routes.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Roads;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.Roads;

public class RoadRepository : IRoadRepository
{
    private readonly AppDataBaseContext _dbc;

    public RoadRepository(AppDataBaseContext dbc)
    {
        _dbc = dbc;
    }

    public async Task<List<TMineRoad>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbc.MineRoads
            .AsNoTracking()
            .OrderBy(r => r.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TMineRoad>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbc.MineRoads
            .AsNoTracking()
            .Where(r => r.Active)
            .OrderBy(r => r.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<TMineRoad?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbc.MineRoads
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbc.MineRoads
            .AnyAsync(r => r.Code == code, cancellationToken);
    }

    public void Add(TMineRoad road)
    {
        _dbc.MineRoads.Add(road);
    }

    public void Remove(TMineRoad road)
    {
        _dbc.MineRoads.Remove(road);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbc.SaveChangesAsync(cancellationToken);
    }
}