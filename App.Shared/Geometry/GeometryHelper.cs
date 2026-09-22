using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace App.Shared.Geometry;

/// <summary>
/// Utilidades de geometria para construir y validar rutas de camiones.
/// El redondeo de esquinas usa Bezier cuadratica (acotada por la tolerancia de
/// la ruta) para que los vehiculos sigan los caminos sin cortar las curvas.
/// </summary>
public static class GeometryHelper
{
    private static readonly NtsGeometryServices _services = NtsGeometryServices.Instance;
    private static readonly GeometryFactory _gf = _services.CreateGeometryFactory(srid: 4326);

    /// <summary>
    /// Redondea las esquinas de una polilinea con Bezier cuadratica (punto de
    /// control = vertice original). Por la propiedad de envolvente convexa de
    /// Bezier, la curva queda dentro del triangulo (p1, vertice, p2), de modo
    /// que el redondeo nunca se sale del corredor delimitado por el radio.
    /// A diferencia del spline Catmull-Rom, aqui la desviacion maxima esta
    /// acotada por maxRadiusM, no es un overshoot libre.
    /// </summary>
    public static LineString RoundCorners(Coordinate[] pts, double maxRadiusM, int samplesPerCorner = 8)
    {
        if (pts.Length < 3)
            return _gf.CreateLineString(pts);

        var outPts = new List<Coordinate> { pts[0] };

        for (int i = 1; i < pts.Length - 1; i++)
        {
            var prev = pts[i - 1];
            var v = pts[i];
            var next = pts[i + 1];

            double segIn = HaversineKm(prev.Y, prev.X, v.Y, v.X) * 1000.0;
            double segOut = HaversineKm(v.Y, v.X, next.Y, next.X) * 1000.0;

            // Nunca mas que el radio pedido, ni mas de 40% del tramo adyacente
            // mas corto (evita comerse esquinas muy proximas entre si).
            double r = Math.Min(maxRadiusM, Math.Min(segIn, segOut) * 0.4);
            if (r < 2.0) { outPts.Add(v); continue; } // esquina muy corta: se deja recta

            var p1 = Lerp(v, prev, r / segIn);
            var p2 = Lerp(v, next, r / segOut);

            outPts.Add(p1);
            for (int s = 1; s < samplesPerCorner; s++)
                outPts.Add(QuadraticBezier(p1, v, p2, (double)s / samplesPerCorner));
            outPts.Add(p2);
        }

        outPts.Add(pts[^1]);
        return _gf.CreateLineString(outPts.ToArray());
    }

    private static Coordinate Lerp(Coordinate from, Coordinate to, double t)
        => new(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t);

    private static Coordinate QuadraticBezier(Coordinate p1, Coordinate v, Coordinate p2, double t)
    {
        double u = 1 - t;
        return new Coordinate(
            u * u * p1.X + 2 * u * t * v.X + t * t * p2.X,
            u * u * p1.Y + 2 * u * t * v.Y + t * t * p2.Y);
    }

    /// <summary>Longitud total de una LineString en km (haversine).</summary>
    public static double LengthKm(LineString line)
    {
        var cs = line.Coordinates;
        if (cs.Length < 2) return 0;
        double total = 0;
        for (int i = 1; i < cs.Length; i++)
            total += HaversineKm(cs[i - 1].Y, cs[i - 1].X, cs[i].Y, cs[i].X);
        return total;
    }

    /// <summary>
    /// Distancia en metros de un punto a la LineString (haversine punto-segmento).
    /// Evita comparar grados contra metros con Geometry.Distance() en SRID 4326.
    /// </summary>
    public static double DistanceM(LineString line, Point point)
    {
        var cs = line.Coordinates;
        if (cs.Length < 2) return double.MaxValue;
        double best = double.MaxValue;
        for (int i = 0; i < cs.Length - 1; i++)
            best = Math.Min(best, PointSegmentDistanceM(point, cs[i], cs[i + 1]));
        return best;
    }

    private static double PointSegmentDistanceM(Point p, Coordinate a, Coordinate b)
    {
        double ax = a.X, ay = a.Y, bx = b.X, by = b.Y;
        double dx = bx - ax, dy = by - ay;
        double len2 = dx * dx + dy * dy;
        double t = len2 > 0 ? Math.Clamp(((p.X - ax) * dx + (p.Y - ay) * dy) / len2, 0, 1) : 0;
        double plon = ax + dx * t, plat = ay + dy * t;
        return HaversineKm(p.Y, p.X, plat, plon) * 1000.0;
    }

    public static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371.0088;
        double dLat = (lat2 - lat1) * Math.PI / 180;
        double dLon = (lon2 - lon1) * Math.PI / 180;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return r * c;
    }
}