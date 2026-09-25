using App.Domain.Routes.Entities;
using App.Domain.Routes.Events;
using App.Shared.Common.Domain;
using NetTopologySuite.Geometries;

namespace App.Domain.Routes.Entities;

public class TRoute : BaseDomain
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public LineString Geometry { get; private set; } = null!;
    public int ToleranceM { get; private set; } = 150;
    public int? MaxSpeedKmh { get; private set; }
    public bool Active { get; private set; } = true;

    /// <summary>Geocerca de origen (carga) asociada a la ruta, si se definió.</summary>
    public Guid? OriginGeofenceId { get; private set; }

    /// <summary>Geocerca de destino (descarga) asociada a la ruta, si se definió.</summary>
    public Guid? DestinationGeofenceId { get; private set; }

    /// <summary>
    /// Límite de velocidad (km/h) por cada tramo de la geometría, serializado
    /// como JSON. Longitud = Geometry.Coordinates.Length - 1.
    /// </summary>
    public string? SpeedProfileJson { get; private set; }

    /// <summary>
    /// Puntos de paso (waypoints) con los que se trazó la ruta, serializados
    /// como JSON. Se persisten para que al editar se reproduzca el mismo trazado
    /// (en lugar de volver a sembrar muestras de la geometría calculada).
    /// </summary>
    public string? WaypointsJson { get; private set; }

    /// <summary>
    /// Identificador del grupo de variantes de una misma ruta (la principal y
    /// sus alternas comparten el mismo RouteGroupId). Null cuando la ruta no
    /// tiene variantes.
    /// </summary>
    public Guid? RouteGroupId { get; private set; }

    /// <summary>Orden dentro del grupo: 0 = principal, 1..n = alternas.</summary>
    public int AlternativeRank { get; private set; }

    private TRoute() { }

    private TRoute(
        string code,
        string name,
        LineString geometry,
        int toleranceM,
        int? maxSpeedKmh,
        Guid? originGeofenceId,
        Guid? destinationGeofenceId,
        string? speedProfileJson,
        string? waypointsJson,
        Guid? routeGroupId,
        int alternativeRank)
    {
        Code = code;
        Name = name;
        Geometry = geometry;
        ToleranceM = toleranceM;
        MaxSpeedKmh = maxSpeedKmh;
        OriginGeofenceId = originGeofenceId;
        DestinationGeofenceId = destinationGeofenceId;
        SpeedProfileJson = speedProfileJson;
        WaypointsJson = waypointsJson;
        RouteGroupId = routeGroupId;
        AlternativeRank = alternativeRank;
        Active = true;
    }

    public static TRoute Create(
        string code,
        string name,
        LineString geometry,
        int toleranceM,
        int? maxSpeedKmh,
        Guid? originGeofenceId,
        Guid? destinationGeofenceId,
        string? speedProfileJson,
        string? waypointsJson = null,
        Guid? routeGroupId = null,
        int alternativeRank = 0)
    {
        var r = new TRoute(
            code,
            name,
            geometry,
            toleranceM,
            maxSpeedKmh,
            originGeofenceId,
            destinationGeofenceId,
            speedProfileJson,
            waypointsJson,
            routeGroupId,
            alternativeRank);

        r.AddDomainEvent(new RouteCreatedEvent(r.Id, r.Code, r.Name, r.OriginGeofenceId, r.DestinationGeofenceId));
        return r;
    }

    public void Update(
        string name,
        LineString geometry,
        int toleranceM,
        int? maxSpeedKmh,
        Guid? originGeofenceId,
        Guid? destinationGeofenceId,
        string? speedProfileJson,
        string? waypointsJson,
        bool active)
    {
        Name = name;
        Geometry = geometry;
        ToleranceM = toleranceM;
        MaxSpeedKmh = maxSpeedKmh;
        OriginGeofenceId = originGeofenceId;
        DestinationGeofenceId = destinationGeofenceId;
        SpeedProfileJson = speedProfileJson;
        WaypointsJson = waypointsJson;
        Active = active;
    }

    public void AssignGroup(Guid? routeGroupId)
    {
        RouteGroupId = routeGroupId;
    }
}