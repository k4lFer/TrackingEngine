namespace App.Objects.Tracking.DTOs.Input.Command;

public class UpdateTripRequest
{
    public Guid? MaterialId { get; set; }

    public decimal? LoadTonnes { get; set; }
}