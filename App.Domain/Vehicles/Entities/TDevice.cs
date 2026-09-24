using App.Shared.Domain;
using App.Shared.Objects.Enums;

namespace App.Domain.Vehicles.Entities;

/// <summary>
/// Registro de dispositivo GPS (Traccar Client del móvil, rastreadores físicos
/// TK103/Teltonika/Queclink…). Se vincula a un vehículo por <see cref="VehicleId"/>
/// (0..1): un identificador es único y un vehículo solo puede tener un dispositivo.
/// </summary>
public class TDevice : BaseDomain
{
    /// <summary>Identificador que reporta el dispositivo (IMEI de 15 dígitos, id del Traccar Client…).</summary>
    public string Identifier { get; private set; } = string.Empty;

    /// <summary>Plataforma/protocolo del dispositivo (Traccar, Tk103, Teltonika, …).</summary>
    public DeviceKind Kind { get; private set; } = DeviceKind.Generic;

    /// <summary>Modelo de hardware (opcional): p. ej. "SinoTrack ST-901", "Traccar Android".</summary>
    public string? Model { get; private set; }

    /// <summary>Vehículo al que está vinculado actualmente (null = dispositivo sin asignar).</summary>
    public Guid? VehicleId { get; private set; }

    private TDevice() { }

    private TDevice(string identifier, DeviceKind kind, string? model)
    {
        Identifier = identifier;
        Kind = kind;
        Model = model;
    }

    public static TDevice Create(string identifier, DeviceKind kind = DeviceKind.Generic, string? model = null)
    {
        return new TDevice(identifier.Trim(), kind, model);
    }

    /// <summary>Asigna el dispositivo a un vehículo (null = desvincular).</summary>
    public void AssignToVehicle(Guid? vehicleId) => VehicleId = vehicleId;

    /// <summary>Actualiza la información de hardware (tipo de protocolo y modelo).</summary>
    public void SetDetails(DeviceKind kind, string? model)
    {
        Kind = kind;
        Model = string.IsNullOrWhiteSpace(model) ? null : model.Trim();
    }
}