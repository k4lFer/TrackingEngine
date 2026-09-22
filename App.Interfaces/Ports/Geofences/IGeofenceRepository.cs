using App.Domain.Geofences.Entities;

namespace App.Interfaces.Ports.Geofences;

public interface IGeofenceRepository
{
    Task<List<TGeofence>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TGeofence>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<TGeofence?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TGeofence?> GetFirstContainingAsync(NetTopologySuite.Geometries.Point position, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);

    void Add(TGeofence geofence);
    void Update(TGeofence geofence);
    void Remove(TGeofence geofence);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}