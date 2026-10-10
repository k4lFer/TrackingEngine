using App.Domain.Positions.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Tracking;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.Tracking;

public class GpsStagingRepository : BaseRepository<TGpsPositionStaging>, IGpsStagingRepository
{
    public GpsStagingRepository(AppDataBaseContext dbc) : base(dbc)
    {
    }

    public async Task SaveAsync(TGpsPositionStaging row, CancellationToken cancellationToken = default)
    {
        await _dbc.GpsPositionsStaging.AddAsync(row, cancellationToken);
        await _dbc.SaveChangesAsync(cancellationToken);
    }

    public Task<List<TGpsPositionStaging>> GetPendingBatchAsync(
        int limit,
        CancellationToken cancellationToken = default)
        => _dbc.GpsPositionsStaging
            .Where(x => !x.Processed)
            .OrderBy(x => x.ReceivedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public Task<TGpsPositionStaging?> GetPendingByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _dbc.GpsPositionsStaging
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<HashSet<(Guid VehicleId, DateTime RecordedAt)>> GetExistingPositionPairsAsync(
        IReadOnlyCollection<Guid> vehicleIds,
        IReadOnlyCollection<DateTime> recordedAt,
        CancellationToken cancellationToken = default)
    {
        if (vehicleIds.Count == 0 || recordedAt.Count == 0)
        {
            return new HashSet<(Guid, DateTime)>();
        }

        var rows = await _dbc.GpsPositions.AsNoTracking()
            .Where(p => vehicleIds.Contains(p.VehicleId) && recordedAt.Contains(p.RecordedAt))
            .Select(p => new { p.VehicleId, p.RecordedAt })
            .ToListAsync(cancellationToken);

        var pairs = new HashSet<(Guid, DateTime)>(rows.Count);
        foreach (var row in rows)
        {
            pairs.Add((row.VehicleId, row.RecordedAt));
        }
        return pairs;
    }
}
