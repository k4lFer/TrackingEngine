using System.Collections.Concurrent;

namespace App.UseCases.Gps.Caching;

public sealed class GpsIngestCache : IGpsIngestCache
{
    private readonly ConcurrentDictionary<string, Guid> _deviceToVehicle = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, DateTime> _vehicleToLastRecorded = new();

    public bool TryGetVehicleId(string deviceId, out Guid vehicleId)
        => _deviceToVehicle.TryGetValue(deviceId, out vehicleId);

    public void SetVehicleId(string deviceId, Guid vehicleId)
        => _deviceToVehicle[deviceId] = vehicleId;

    public void RemoveDevice(string deviceId)
        => _deviceToVehicle.TryRemove(deviceId, out _);

    public bool ShouldSkipByRecordedAt(Guid vehicleId, DateTime recordedAt, TimeSpan epsilon)
    {
        if (recordedAt == default) return false;
        if (_vehicleToLastRecorded.TryGetValue(vehicleId, out var last))
        {
            if (recordedAt <= last + epsilon) return true;
        }
        return false;
    }

    public void UpdateLastRecordedAt(Guid vehicleId, DateTime recordedAt)
    {
        if (recordedAt == default) return;
        _vehicleToLastRecorded.AddOrUpdate(vehicleId, recordedAt, (_, existing) => recordedAt > existing ? recordedAt : existing);
    }

    public void RemoveVehicle(Guid vehicleId)
        => _vehicleToLastRecorded.TryRemove(vehicleId, out _);
}
