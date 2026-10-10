using App.Domain.Positions.Entities;

namespace App.Interfaces.Ports.Tracking;

public interface IGpsStagingRepository
{
    Task SaveAsync(TGpsPositionStaging row, CancellationToken cancellationToken = default);

    Task<List<TGpsPositionStaging>> GetPendingBatchAsync(
        int limit,
        CancellationToken cancellationToken = default);

    Task<TGpsPositionStaging?> GetPendingByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<HashSet<(Guid VehicleId, DateTime RecordedAt)>> GetExistingPositionPairsAsync(
        IReadOnlyCollection<Guid> vehicleIds,
        IReadOnlyCollection<DateTime> recordedAt,
        CancellationToken cancellationToken = default);
}
