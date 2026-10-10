namespace App.UseCases.Gps.Caching;

public interface IGpsIngestCache
{
    bool TryGetVehicleId(string deviceId, out Guid vehicleId);
    void SetVehicleId(string deviceId, Guid vehicleId);
    void RemoveDevice(string deviceId);
    bool ShouldSkipByRecordedAt(Guid vehicleId, DateTime recordedAt, TimeSpan epsilon);
    void UpdateLastRecordedAt(Guid vehicleId, DateTime recordedAt);
    void RemoveVehicle(Guid vehicleId);
}
