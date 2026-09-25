using System.Globalization;
using NetTopologySuite.Geometries;
using GeoType = NetTopologySuite.Geometries.Geometry;

namespace App.Shared.Utils.Geometry;

public static class GeoJsonConverter
{
    public static string ToGeoJson(GeoType geometry) => geometry switch
    {
        LineString line => $"{{\"type\":\"LineString\",\"coordinates\":{C(line.Coordinates)}}}",
        Polygon poly => $"{{\"type\":\"Polygon\",\"coordinates\":[{C(poly.ExteriorRing.Coordinates)}]}}",
        Point point => $"{{\"type\":\"Point\",\"coordinates\":[{F(point.X)},{F(point.Y)}]}}",
        _ => "null"
    };

    private static string C(Coordinate[] coords)
        => "[" + string.Join(",", coords.Select(c => $"[{F(c.X)},{F(c.Y)}]")) + "]";

    private static string F(double d) => d.ToString("G", CultureInfo.InvariantCulture);
}