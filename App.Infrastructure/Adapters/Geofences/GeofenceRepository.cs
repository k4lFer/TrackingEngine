using App.Domain.Geofences.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Geofences;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace App.Infrastructure.Adapters.Geofences;

public class GeofenceRepository : IGeofenceRepository
{
    private readonly AppDataBaseContext _dbc;

    public GeofenceRepository(AppDataBaseContext dbc)
    {
        _dbc = dbc;
    }

    public async Task<List<TGeofence>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbc.Geofences
            .AsNoTracking()
            .OrderBy(g => g.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TGeofence>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbc.Geofences
            .AsNoTracking()
            .Where(g => g.Active)
            .OrderBy(g => g.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<TGeofence?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbc.Geofences
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<TGeofence?> GetFirstContainingAsync(Point position, CancellationToken cancellationToken = default)
    {
        return await _dbc.Geofences
            .Where(g => g.Active && g.Geometry.Contains(position))
            .OrderBy(g => g.Priority)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbc.Geofences
            .AnyAsync(g => g.Code == code, cancellationToken);
    }

    public void Add(TGeofence geofence)
    {
        _dbc.Geofences.Add(geofence);
    }

    public void Update(TGeofence geofence)
    {
        _dbc.Geofences.Update(geofence);
    }

    public void Remove(TGeofence geofence)
    {
        _dbc.Geofences.Remove(geofence);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbc.SaveChangesAsync(cancellationToken);
    }
}