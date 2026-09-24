namespace App.Objects.Tracking.DTOs.Input.Command;

public class CreateTripRequest
{
    public Guid VehicleId { get; set; }

    public Guid? OriginGeofenceId { get; set; }

    public Guid? DestinationGeofenceId { get; set; }

    public Guid? RouteId { get; set; }

    public Guid? MaterialId { get; set; }

    public decimal? LoadTonnes { get; set; }
}