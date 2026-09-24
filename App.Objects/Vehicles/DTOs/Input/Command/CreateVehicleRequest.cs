namespace App.Objects.Vehicles.DTOs.Input.Command;

public class CreateVehicleRequest
{
    public string Code { get; set; }
    public string Plate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }

    /// <summary>Identificador del dispositivo a vincular (IMEI / id del Traccar); vacío = sin dispositivo.</summary>
    public string? DeviceIdentifier { get; set; }

    /// <summary>Tipo/protocolo del dispositivo (DeviceKind); nulo = por defecto.</summary>
    public int? DeviceKind { get; set; }

    /// <summary>Modelo de hardware del dispositivo (opcional).</summary>
    public string? DeviceModel { get; set; }
}