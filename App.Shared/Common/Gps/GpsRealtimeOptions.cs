namespace App.Shared.Common.Gps;

public sealed class GpsRealtimeOptions
{
    public int LongStopThresholdSeconds { get; set; } = 300;
    public decimal StopSpeedKmh { get; set; } = 3;
    public int StateMinSeconds { get; set; } = 10;
    public double MinDistanceMeters { get; set; } = 30;
    public int MaxSuppressSeconds { get; set; } = 120;
    public int HeartbeatSeconds { get; set; } = 30;
    public int MaxTrackedVehicles { get; set; } = 5000;
    public int OfflineThresholdSeconds { get; set; } = 300;
    public int StaleScanIntervalSeconds { get; set; } = 60;
}
