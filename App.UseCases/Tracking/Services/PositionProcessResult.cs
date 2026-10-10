using App.Domain.Tracking.Entities;
using App.Domain.Vehicles.Entities;

namespace App.UseCases.Tracking.Services;

public sealed class PositionProcessResult
{
    public bool IsValid { get; set; }
    public string? Error { get; set; }
    public TGpsPosition? Position { get; set; }
    public TVehicleCurrentState? State { get; set; }
    public List<TTrackingEvent> Events { get; set; } = new();
    public bool IsRetroactive { get; set; }
    public DateTime DeviceTimestamp { get; set; }
}
