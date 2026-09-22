using App.Domain.Geofences.Events;
using App.Shared.Domain;
using App.Shared.Objects.Enums;
using NetTopologySuite.Geometries;

namespace App.Domain.Geofences.Entities;

public class TGeofence : BaseDomain
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public GeofenceKind Kind { get; private set; }
    public int Priority { get; private set; } = 100;
    public Polygon Geometry { get; private set; } = null!;
    public decimal? MaxSpeedKmh { get; private set; }
    public string Color { get; private set; } = "#6c757d";
    public bool Active { get; private set; } = true;

    private TGeofence() { }

    private TGeofence(
        string code,
        string name,
        GeofenceKind kind,
        int priority,
        Polygon geometry,
        decimal? maxSpeedKmh,
        string color)
    {
        Code = code;
        Name = name;
        Kind = kind;
        Priority = priority;
        Geometry = geometry;
        MaxSpeedKmh = maxSpeedKmh;
        Color = color;
        Active = true;
    }

    public static TGeofence Create(
        string code,
        string name,
        GeofenceKind kind,
        int priority,
        Polygon geometry,
        decimal? maxSpeedKmh,
        string? color)
    {
        var g = new TGeofence(
            code,
            name,
            kind,
            priority,
            geometry,
            maxSpeedKmh,
            string.IsNullOrWhiteSpace(color) ? "#6c757d" : color);

        g.AddDomainEvent(new GeofenceCreatedEvent(g.Id, g.Code, g.Name));
        return g;
    }

    public void Update(
        string name,
        GeofenceKind kind,
        int priority,
        Polygon geometry,
        decimal? maxSpeedKmh,
        bool active)
    {
        Name = name;
        Kind = kind;
        Priority = priority;
        Geometry = geometry;
        MaxSpeedKmh = maxSpeedKmh;
        Active = active;
    }
}