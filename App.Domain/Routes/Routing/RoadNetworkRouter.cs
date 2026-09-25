using App.Domain.Routes.Entities;
using App.Shared.Utils.Geometry;
using App.Shared.Common.Routing;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace App.Domain.Routes.Routing;

/// <summary>
/// Enruta una secuencia de waypoints a través de la red vial interna de la mina.
/// En lugar de unir los puntos en línea recta, proyecta (ortogonalmente) cada
/// punto al camino más cercano y calcula la ruta más corta (Dijkstra ponderado
/// por tiempo) sobre el grafo formado por los caminos. Cada arista pondera por
/// tiempo = distancia / velocidad del camino, de modo que los camiones prefieren
/// las vías más rápidas en lugar de las más cortas.
/// </summary>
public static class RoadNetworkRouter
{
    private const double SnapMeters = 45.0;
    public const double DefaultSpeedKmh = 40.0;

    private static readonly NtsGeometryServices _services =
        NtsGeometryServices.Instance;
    private static readonly GeometryFactory _gf =
        _services.CreateGeometryFactory(srid: 4326);

    public static LineString? Route(
        IReadOnlyList<TMineRoad> roads,
        IReadOnlyList<GeoCoordinate> waypoints)
    {
        if (roads.Count == 0 || waypoints.Count < 2) return null;

        var graph = new Graph();
        graph.AddRoads(roads);

        var terminals = graph.SnapWaypoints(waypoints);
        if (terminals.Count < 2) return null;

        // Encadena el camino más corto entre cada par de waypoints consecutivos,
        // de modo que la ruta recorra la red pasando por los puntos de paso en orden.
        var result = new List<Coordinate>();
        for (int i = 0; i < terminals.Count - 1; i++)
        {
            var path = graph.ShortestPath(terminals[i], terminals[i + 1]);
            if (path is null || path.Count < 2) return null;
            result.AddRange(i == 0 ? path : path.Skip(1));
        }
        if (result.Count < 2) return null;

        return _gf.CreateLineString(result.ToArray());
    }

    /// <summary>
    /// Límite de velocidad (km/h) de cada tramo de la geometría final de la ruta.
    /// Cada tramo se asigna a la vía más cercana (proyectando su punto medio) y
    /// toma su MaxSpeedKmh. Coincide con los tramos redondeados por RoundCorners.
    /// </summary>
    public static double[]? SpeedProfileFor(IReadOnlyList<TMineRoad> roads, LineString geometry)
    {
        var cs = geometry.Coordinates;
        var roadsSegs = roadSegments(roads);
        if (roadsSegs.Count == 0 || cs.Length < 2) return null;

        var profile = new double[cs.Length - 1];
        for (int i = 0; i < cs.Length - 1; i++)
            profile[i] = NearestRoad(roadsSegs, cs[i], cs[i + 1]).Speed;
        return profile;
    }

    /// <summary>
    /// Genera instrucciones paso a paso para una ruta calculada sobre la red
    /// interna de la mina. Recorre la geometría y emite una maniobra cada vez que
    /// el camino cambia o el rumbo gira más de ~35°, reproducendo el contrato de
    /// Valhalla (maneuvers con instrucción, calle y longitud/tiempo del tramo).
    /// </summary>
    public static IReadOnlyList<RouteManeuver> ManeuversFor(
        IReadOnlyList<TMineRoad> roads, LineString geometry)
    {
        var cs = geometry.Coordinates;
        int segCount = cs.Length - 1;
        if (segCount < 1) return Array.Empty<RouteManeuver>();

        var segs = roadSegments(roads);
        if (segs.Count == 0) return Array.Empty<RouteManeuver>();

        var headings = new double[segCount];
        var names = new string[segCount];
        var speeds = new double[segCount];
        for (int i = 0; i < segCount; i++)
        {
            headings[i] = HeadingDeg(cs[i], cs[i + 1]);
            var nearest = NearestRoad(segs, cs[i], cs[i + 1]);
            names[i] = nearest.Name;
            speeds[i] = nearest.Speed;
        }

        var maneuvers = new List<RouteManeuver>();
        int run = 0;
        for (int i = 0; i < segCount; i++)
        {
            string? turnWord = null;
            bool split = i + 1 < segCount
                && !string.Equals(names[i], names[i + 1], StringComparison.OrdinalIgnoreCase);

            if (i + 1 < segCount)
            {
                double delta = NormalizeDeg(headings[i + 1] - headings[i]);
                if (Math.Abs(delta) > 22.0)
                    turnWord = delta > 0 ? "a la derecha" : "a la izquierda";
            }

            if (i == segCount - 1 || turnWord is not null || split)
            {
                maneuvers.Add(MakeManeuver(cs, run, i, speeds, names, turnWord, i == segCount - 1));
                run = i + 1;
            }
        }
        return maneuvers;
    }

    private static RouteManeuver MakeManeuver(
        Coordinate[] cs, int start, int end, double[] speeds, string[] names,
        string? turnWord, bool last)
    {
        double km = 0;
        double seconds = 0;
        for (int i = start; i <= end; i++)
        {
            double lenM = HaversineM(cs[i].Y, cs[i].X, cs[i + 1].Y, cs[i + 1].X);
            km += lenM / 1000.0;
            seconds += speeds[i] > 0 ? lenM / (speeds[i] / 3.6) : 0;
        }

        string street = string.IsNullOrWhiteSpace(names[start]) ? "la vía" : names[start];
        string instruction = last
            ? "Llegada al destino"
            : turnWord is not null
                ? $"Gire {turnWord} hacia {street}"
                : start == 0
                    ? $"Saliendo de la operación, continúe por {street}"
                    : $"Continúe por {street}";

        int type = last ? 2 : turnWord is not null ? (turnWord == "a la derecha" ? 3 : 4) : start == 0 ? 1 : 0;
        return new RouteManeuver(type, instruction, new[] { names[start] }, km, seconds);
    }

    private static double HeadingDeg(Coordinate a, Coordinate b)
    {
        double dLon = (b.X - a.X) * Math.Cos(a.Y * Math.PI / 180.0);
        double dLat = b.Y - a.Y;
        double deg = Math.Atan2(dLon, dLat) * 180.0 / Math.PI;
        return deg < 0 ? deg + 360.0 : deg;
    }

    private static double NormalizeDeg(double deg)
    {
        while (deg > 180.0) deg -= 360.0;
        while (deg < -180.0) deg += 360.0;
        return deg;
    }

    private readonly record struct RoadSegment(
        double X1, double Y1, double X2, double Y2, double Speed, string Name);

    private static List<RoadSegment> roadSegments(IReadOnlyList<TMineRoad> roads)
    {
        var list = new List<RoadSegment>();
        foreach (var road in roads)
        {
            double speed = road.MaxSpeedKmh is > 0 ? road.MaxSpeedKmh.Value : DefaultSpeedKmh;
            var cs = road.Geometry.Coordinates;
            for (int i = 0; i < cs.Length - 1; i++)
                list.Add(new RoadSegment(cs[i].X, cs[i].Y, cs[i + 1].X, cs[i + 1].Y, speed, road.Name));
        }
        return list;
    }

    private static RoadSegment NearestRoad(
        List<RoadSegment> segs, Coordinate p1, Coordinate p2)
    {
        double midLat = (p1.Y + p2.Y) / 2;
        double midLon = (p1.X + p2.X) / 2;
        RoadSegment best = default;
        double bestDist = double.MaxValue;
        foreach (var s in segs)
        {
            double ax = s.X1, ay = s.Y1, bx = s.X2, by = s.Y2;
            double dx = bx - ax, dy = by - ay;
            double len2 = dx * dx + dy * dy;
            double t = len2 > 0 ? Math.Clamp(((midLon - ax) * dx + (midLat - ay) * dy) / len2, 0, 1) : 0;
            double plon = ax + dx * t, plat = ay + dy * t;
            double d = HaversineM(midLat, midLon, plat, plon);
            if (d < bestDist)
            {
                bestDist = d;
                best = s;
            }
        }
        return best;
    }

    public static double HaversineM(double lat1, double lon1, double lat2, double lon2)
        => GeometryHelper.HaversineKm(lat1, lon1, lat2, lon2) * 1000.0;

    private sealed class Node
    {
        public double Lon;
        public double Lat;
        public readonly List<(Node n, double w)> Adjacent = new();
        public Node(double lon, double lat) { Lon = lon; Lat = lat; }
    }

    private sealed class Segment
    {
        public Node A = null!;
        public Node B = null!;
        public double Len;
        public double SpeedKmh = DefaultSpeedKmh;

        public void Link()
        {
            Len = HaversineM(A.Lat, A.Lon, B.Lat, B.Lon);
            double time = Len / (SpeedKmh / 3.6); // segundos
            A.Adjacent.Add((B, time));
            B.Adjacent.Add((A, time));
        }
        public void Unlink()
        {
            A.Adjacent.RemoveAll(x => x.n == B);
            B.Adjacent.RemoveAll(x => x.n == A);
        }
    }

    private sealed class Graph
    {
        private readonly Dictionary<long, Node> _nodes = new();
        private readonly List<Segment> _segments = new();

        private static long KeyOf(double lon, double lat)
        {
            long x = (long)Math.Round(lon / 1e-6);
            long y = (long)Math.Round(lat / 1e-6);
            return (x << 32) | (y & 0xFFFFFFFFL);
        }

        private Node GetOrCreate(double lon, double lat)
        {
            var key = KeyOf(lon, lat);
            if (_nodes.TryGetValue(key, out var existing)) return existing;
            var node = new Node(lon, lat);
            _nodes[key] = node;
            return node;
        }

        public void AddRoads(IReadOnlyList<TMineRoad> roads)
        {
            foreach (var road in roads)
            {
                double speed = road.MaxSpeedKmh is > 0 ? road.MaxSpeedKmh.Value : DefaultSpeedKmh;
                var coords = road.Geometry.Coordinates;
                if (coords.Length < 2) continue;
                for (int i = 0; i < coords.Length - 1; i++)
                {
                    var a = GetOrCreate(coords[i].X, coords[i].Y);
                    var b = GetOrCreate(coords[i + 1].X, coords[i + 1].Y);
                    if (HaversineM(a.Lat, a.Lon, b.Lat, b.Lon) < 1) continue;
                    var seg = new Segment { A = a, B = b, SpeedKmh = speed };
                    seg.Link();
                    _segments.Add(seg);
                }
            }
        }

        public List<Node> SnapWaypoints(IReadOnlyList<GeoCoordinate> waypoints)
        {
            var terminals = new List<Node>();
            foreach (var wp in waypoints)
            {
                var node = SnapOne(wp.Lat, wp.Lon);
                if (node is not null) terminals.Add(node);
            }
            return terminals;
        }

        private Node? SnapOne(double lat, double lon)
        {
            double bestD = double.MaxValue;
            Segment? bestSeg = null;
            double bestT = 0;
            double bestPLon = 0, bestPLat = 0;

            foreach (var seg in _segments)
            {
                double ax = seg.A.Lon, ay = seg.A.Lat;
                double bx = seg.B.Lon, by = seg.B.Lat;
                double dx = bx - ax, dy = by - ay;
                double dot = (lon - ax) * dx + (lat - ay) * dy;
                double len2 = dx * dx + dy * dy;
                double t = len2 > 0 ? Math.Clamp(dot / len2, 0, 1) : 0;
                double plon = ax + dx * t;
                double plat = ay + dy * t;
                double d = HaversineM(lat, lon, plat, plon);
                if (d < bestD)
                {
                    bestD = d;
                    bestSeg = seg;
                    bestT = t;
                    bestPLon = plon;
                    bestPLat = plat;
                }
            }

            if (bestSeg is null || bestD > SnapMeters) return null;

            if (bestT <= 0.001 || bestT >= 0.999)
            {
                return bestT <= 0.001 ? bestSeg.A : bestSeg.B;
            }

            var existing = GetOrCreate(bestPLon, bestPLat);
            if (!ReferenceEquals(existing, bestSeg.A) && !ReferenceEquals(existing, bestSeg.B))
            {
                bestSeg.Unlink();
                _segments.Remove(bestSeg);

                var left = new Segment { A = bestSeg.A, B = existing, SpeedKmh = bestSeg.SpeedKmh };
                var right = new Segment { A = existing, B = bestSeg.B, SpeedKmh = bestSeg.SpeedKmh };
                left.Link();
                right.Link();
                _segments.Add(left);
                _segments.Add(right);
            }
            return existing;
        }

        public List<Coordinate>? ShortestPath(Node start, Node end)
        {
            var dist = new Dictionary<Node, double>();
            var prev = new Dictionary<Node, Node>();
            var visited = new HashSet<Node>();
            var pq = new SortedSet<(double, Node)>(Comparer<(double, Node)>.Create((x, y) =>
            {
                int c = x.Item1.CompareTo(y.Item1);
                return c != 0 ? c : ReferenceComparer.Compare(x.Item2, y.Item2);
            }))
            {
                (0, start)
            };
            dist[start] = 0;

            while (pq.Count > 0)
            {
                var (d, u) = pq.Min;
                pq.Remove(pq.Min);
                if (visited.Contains(u)) continue;
                visited.Add(u);
                if (ReferenceEquals(u, end)) break;

                foreach (var (v, w) in u.Adjacent)
                {
                    if (visited.Contains(v)) continue;
                    double nd = d + w;
                    if (!dist.TryGetValue(v, out var cd) || nd < cd)
                    {
                        dist[v] = nd;
                        prev[v] = u;
                        pq.Add((nd, v));
                    }
                }
            }

            if (!ReferenceEquals(start, end) && !visited.Contains(end)) return null;

            var pathNodes = new List<Node>();
            Node? cur = end;
            while (cur is not null)
            {
                pathNodes.Add(cur);
                if (ReferenceEquals(cur, start)) break;
                cur = prev.TryGetValue(cur, out var p) ? p : null;
            }
            pathNodes.Reverse();

            var result = new List<Coordinate>();
            foreach (var n in pathNodes)
                result.Add(new Coordinate(n.Lon, n.Lat));
            return result;
        }

        private static readonly NodeReferenceComparer ReferenceComparer = new();
        private sealed class NodeReferenceComparer : IComparer<Node>
        {
            public int Compare(Node? x, Node? y) => ReferenceEquals(x, y) ? 0 : (System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(x!) < System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(y!) ? -1 : 1);
        }
    }
}