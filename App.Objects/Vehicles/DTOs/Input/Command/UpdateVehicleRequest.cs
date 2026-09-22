namespace App.Objects.Vehicles.DTOs.Input.Command;

public class UpdateVehicleRequest
{
    public string Plate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public bool Active { get; set; }
}