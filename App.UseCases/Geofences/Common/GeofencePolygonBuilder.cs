using App.Objects.Shared.DTOs;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace App.UseCases.Geofences.Common;

public static class GeofencePolygonBuilder
{
    private static readonly GeometryFactory _gf = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public static Polygon Build(List<CoordinateDto> polygon)
    {
        var coords = polygon.Select(c => new Coordinate(c.Lon, c.Lat)).ToArray();

        if (coords[0] != coords[^1])
        {
            var closed = new Coordinate[coords.Length + 1];
            coords.CopyTo(closed, 0);
            closed[^1] = coords[0];
            coords = closed;
        }

        return _gf.CreatePolygon(coords);
    }
}