namespace App.Objects.Vehicles.DTOs.Input.Command;

public class CreateVehicleRequest
{
    public string Code { get; set; }
    public string Plate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
}