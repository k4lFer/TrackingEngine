using App.Domain.Vehicles.Events;
using App.Shared.Domain;
using NetTopologySuite.Geometries;

namespace App.Domain.Vehicles.Entities;

public class TVehicle : BaseDomain
{
    public string Code { get; private set; } = string.Empty;
    public string Plate { get; private set; } = string.Empty;
    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public bool Active { get; private set; } = true;
    public Point? LastPosition { get; private set; }
    public DateTime? LastReportedAt { get; private set; }

    public TVehicleCurrentState? CurrentState { get; private set; }

    private TVehicle() { }

    private TVehicle(string code, string plate, string brand, string model)
    {
        Code = code;
        Plate = plate;
        Brand = brand;
        Model = model;
    }

    public static TVehicle Create(string code, string plate, string brand, string model)
    {
        var v = new TVehicle(code, plate, brand, model);
        v.AddDomainEvent(new VehicleCreatedEvent(v.Id, v.Code, v.Plate, v.Brand, v.Model));
        return v;
    }

    public void Update(string plate, string brand, string model, bool active)
    {
        Plate = plate;
        Brand = brand;
        Model = model;
        Active = active;
        AddDomainEvent(new VehicleUpdatedEvent(Id, Code, Plate, Brand, Model, Active));
    }

    /// <summary>
    /// Apaga el vehículo cuando queda sin dispositivo: descarta la última posición
    /// y resetea el estado en curso (Offline, sin viaje ni geofence activos).
    /// </summary>
    public void GoOffline()
    {
        LastPosition = null;
        LastReportedAt = null;
        CurrentState?.ResetToOffline();
    }

    public void RecordLastPosition(Point? position, DateTime reportedAt)
    {
        LastPosition = position;
        LastReportedAt = reportedAt;
    }

    public void AttachState(TVehicleCurrentState state) => CurrentState = state;
}