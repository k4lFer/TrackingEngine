namespace App.Objects.Tracking.DTOs.Input.Command;

public class PositionReportRequest
{
    public Guid VehicleId { get; set; }
    public string? DeviceId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal? SpeedKmh { get; set; }
    public short? HeadingDeg { get; set; }
    public bool? Ignition { get; set; }
    public decimal? OdometerKm { get; set; }
    public decimal? Hdop { get; set; }
    public short? Satellites { get; set; }
    public DateTime RecordedAt { get; set; }
}