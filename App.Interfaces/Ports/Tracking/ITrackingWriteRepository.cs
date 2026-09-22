using App.Domain.Tracking.Entities;

namespace App.Interfaces.Ports.Tracking;

public interface ITrackingWriteRepository
{
    Task<TTrip?> GetActiveTripByVehicleAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<List<TTrip>> GetAllActiveAsync(
        CancellationToken cancellationToken = default);

    void Add(TTrip trip);
    void Add(TGpsPosition position);
    void Add(TTrackingEvent trackingEvent);
    void Update(TTrip trip);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}