using App.Domain.Routes.Events;
using App.Shared.Common.Domain;
using NetTopologySuite.Geometries;

namespace App.Domain.Routes.Entities;

/// <summary>
/// Un camino de la red vial interna de la mina. Representa una arista del grafo
/// por la que se enrutan los camiones. La intersección de extremos entre varios
/// caminos forma la red sobre la que se calcula el trayecto (en lugar de ir en
/// línea recta por encima del terreno).
/// </summary>
public class TMineRoad : BaseDomain
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public LineString Geometry { get; private set; } = null!;
    public int? MaxSpeedKmh { get; private set; }
    public bool Active { get; private set; } = true;

    private TMineRoad() { }

    private TMineRoad(string code, string name, LineString geometry, int? maxSpeedKmh)
    {
        Code = code;
        Name = name;
        Geometry = geometry;
        MaxSpeedKmh = maxSpeedKmh;
        Active = true;
    }

    public static TMineRoad Create(string code, string name, LineString geometry, int? maxSpeedKmh)
    {
        var r = new TMineRoad(code, name, geometry, maxSpeedKmh);
        r.AddDomainEvent(new MineRoadCreatedEvent(r.Id, r.Code, r.Name));
        return r;
    }
}