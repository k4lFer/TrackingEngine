namespace App.Objects.Vehicles.DTOs.Input.Command;

public class UpdateVehicleRequest
{
    public string Plate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public bool Active { get; set; }

    /// <summary>Identificador del dispositivo a vincular (IMEI / id del Traccar); vacío/nulo = desvincular.</summary>
    public string? DeviceIdentifier { get; set; }

    /// <summary>Tipo/protocolo del dispositivo (DeviceKind); nulo = no modificar.</summary>
    public int? DeviceKind { get; set; }

    /// <summary>Modelo de hardware del dispositivo (opcional); nulo/vacío = sin modelo.</summary>
    public string? DeviceModel { get; set; }
}