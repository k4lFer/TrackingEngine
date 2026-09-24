using App.Domain.Geofences.Entities;
using App.Domain.Materials.Entities;
using App.Domain.Routes.Entities;
using App.Domain.Routes.Routing;
using App.Domain.Tracking.Entities;
using App.Domain.Vehicles.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Shared.Geometry;
using App.Shared.Objects.Enums;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace App.Infrastructure.Core.DataBaseContext.Seed;

/// <summary>
/// Semilla de demo replicando el seed del backend demo (TrackingEngine.Demo).
/// Distrito minero cerca de Cusco: zona de carga, planta de descarga,
/// red vial interna y tres rutas por los caminos reales. Idempotente:
/// si ya existen vehículos, no vuelve a sembrar.
/// </summary>
public static class DataSeeder
{
    private static readonly GeometryFactory _gf = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
    private static readonly Random _rng = new(42);
    private const int DemoRouteToleranceM = 150;

    public static async Task InitializeAsync(AppDataBaseContext db)
    {
        if (await db.Vehicles.AnyAsync())
            return;

        var vehicle1 = TVehicle.Create("CAM-023", "ABC-123", "Caterpillar", "785D");
        var vehicle2 = TVehicle.Create("CAM-031", "DEF-456", "Komatsu", "HD785");
        var vehicle3 = TVehicle.Create("CAM-045", "GHI-789", "Hitachi", "EH4000");
        db.Vehicles.AddRange(vehicle1, vehicle2, vehicle3);
        await db.SaveChangesAsync();

        var mineralCobre = TMaterial.Create("MAT-CU", "Mineral de Cobre", "ton");
        var mineralZinc = TMaterial.Create("MAT-ZN", "Mineral de Zinc", "ton");
        var desmonte = TMaterial.Create("MAT-DES", "Desmonte", "ton");
        db.Materials.AddRange(mineralCobre, mineralZinc, desmonte);
        await db.SaveChangesAsync();

        var zonaCarga = TGeofence.Create(
            "MINA-A-CARGA", "Zona de Carga - Mina A", GeofenceKind.Load, 10,
            CreatePolygon(-13.6400, -72.8900, -13.6350, -72.8850), 20, "#28a745");
        var zonaDescarga = TGeofence.Create(
            "PLANTA-B-DESCARGA", "Planta de Procesamiento B", GeofenceKind.Unload, 10,
            CreatePolygon(-13.6100, -72.8600, -13.6050, -72.8550), 15, "#dc3545");
        var checkpoint1 = TGeofence.Create(
            "CHECKPOINT-01", "Punto Control - Km 15", GeofenceKind.Checkpoint, 50,
            CreatePolygon(-13.6250, -72.8770, -13.6240, -72.8760), null, "#17a2b8");
        var zonaEspera = TGeofence.Create(
            "ESPERA-PLANTA", "Zona de Espera Planta B", GeofenceKind.Wait, 30,
            CreatePolygon(-13.6150, -72.8650, -13.6130, -72.8630), null, "#ffc107");
        var zonaRestringida = TGeofence.Create(
            "RESTRINGIDO-01", "Zona Restringida - Presa", GeofenceKind.Restricted, 1,
            CreatePolygon(-13.6200, -72.8700, -13.6190, -72.8690), null, "#6f42c1");
        db.Geofences.AddRange(zonaCarga, zonaDescarga, checkpoint1, zonaEspera, zonaRestringida);
        await db.SaveChangesAsync();

        var network = CreateRoadNetwork();
        db.MineRoads.AddRange(network);
        await db.SaveChangesAsync();

        var routeAGeometry = BuildRouteGeometry(network, RouteACoordinateRoute(), DemoRouteToleranceM);
        var routeBGeometry = BuildRouteGeometry(network, RouteBCoordinateRoute(), DemoRouteToleranceM);
        var routeCGeometry = BuildRouteGeometry(network, RouteCCoordinateRoute(), DemoRouteToleranceM);

        var routeA = TRoute.Create(
            "RUTA-MINA-A-PLANTA-B", "Ruta Mina-A -> Planta-B (Este)",
            routeAGeometry, DemoRouteToleranceM, 60, zonaCarga.Id, zonaDescarga.Id,
            BuildRouteSpeedProfileJson(network, routeAGeometry));
        var routeB = TRoute.Create(
            "RUTA-02-OESTE", "Ruta Mina-A -> Planta-B (Oeste)",
            routeBGeometry, DemoRouteToleranceM, 55, zonaCarga.Id, zonaDescarga.Id,
            BuildRouteSpeedProfileJson(network, routeBGeometry));
        var routeC = TRoute.Create(
            "RUTA-03-NORTE", "Ruta Mina-A -> Planta-B (Norte)",
            routeCGeometry, DemoRouteToleranceM, 50, zonaCarga.Id, zonaDescarga.Id,
            BuildRouteSpeedProfileJson(network, routeCGeometry));
        db.Routes.AddRange(routeA, routeB, routeC);
        await db.SaveChangesAsync();

        var trip1 = TTrip.Start(vehicle1.Id, zonaCarga.Id, zonaDescarga.Id, routeA.Id, mineralCobre.Id, 120);
        var trip2 = TTrip.Start(vehicle2.Id, zonaCarga.Id, zonaDescarga.Id, routeB.Id, mineralZinc.Id, 75);
        var trip3 = TTrip.Start(vehicle3.Id, zonaCarga.Id, zonaDescarga.Id, routeC.Id, desmonte.Id, 185);
        db.Trips.AddRange(trip1, trip2, trip3);
        await db.SaveChangesAsync();

        await GenerateVehiclePositions(
            db, vehicle1, routeA.Geometry.Coordinates.Select(c => (c.Y, c.X)).ToArray(),
            DateTime.UtcNow.AddMinutes(-50), 0);
        await GenerateVehiclePositions(
            db, vehicle2, routeB.Geometry.Coordinates.Select(c => (c.Y, c.X)).ToArray(),
            DateTime.UtcNow.AddMinutes(-42), 1);
        await GenerateVehiclePositions(
            db, vehicle3, routeC.Geometry.Coordinates.Select(c => (c.Y, c.X)).ToArray(),
            DateTime.UtcNow.AddMinutes(-34), 2);
    }

    private static List<TMineRoad> CreateRoadNetwork()
    {
        return new List<TMineRoad>
        {
            TMineRoad.Create("VIA-CENTRAL", "Vía central",
                Line(ToCoords(MINA, C_NORTE, C_CENTRAL, C_SUR, C_PLANTA, PLANTA)), 50),
            TMineRoad.Create("VIA-NORTE", "Vía norte",
                Line(ToCoords(MINA, C_NORTE, N1, N2, N3, N4, PLANTA)), 45),
            TMineRoad.Create("VIA-SUR", "Vía sur",
                Line(ToCoords(MINA, S1, S2, S3, S4, PLANTA)), 40)
        };
    }

    private static LineString BuildRouteGeometry(
        IReadOnlyList<TMineRoad> roads, List<GeoCoordinate> waypoints, int toleranceM)
    {
        var routed = RoadNetworkRouter.Route(roads, waypoints);
        if (routed is not null && routed.Coordinates.Length >= 2)
            return GeometryHelper.RoundCorners(routed.Coordinates, maxRadiusM: toleranceM);
        var coords = waypoints.Select(c => new Coordinate(c.Lon, c.Lat)).ToArray();
        return GeometryHelper.RoundCorners(coords, maxRadiusM: toleranceM);
    }

    private static string? BuildRouteSpeedProfileJson(
        IReadOnlyList<TMineRoad> roads, LineString geometry)
    {
        var speeds = RoadNetworkRouter.SpeedProfileFor(roads, geometry);
        if (speeds is null || speeds.Length == 0) return null;
        return System.Text.Json.JsonSerializer.Serialize(speeds);
    }

    private static async Task GenerateVehiclePositions(
        AppDataBaseContext db, TVehicle vehicle,
        (double Lat, double Lon)[] routePoints, DateTime startTime, int vehicleIndex)
    {
        var positions = new List<TGpsPosition>();
        var odometer = 125430m + vehicleIndex * 200m;

        for (int i = 0; i < routePoints.Length; i++)
        {
            var (baseLat, baseLon) = routePoints[i];

            var jitterLat = (_rng.NextDouble() - 0.5) * 0.0001;
            var jitterLon = (_rng.NextDouble() - 0.5) * 0.0001;
            var lat = baseLat + jitterLat;
            var lon = baseLon + jitterLon;

            double speed;
            double progress = (double)i / (routePoints.Length - 1);

            if (progress < 0.08)
                speed = progress / 0.08 * 35;
            else if (progress < 0.15)
                speed = 35 + (progress - 0.08) / 0.07 * 20;
            else if (progress < 0.80)
                speed = 45 + Math.Sin(progress * Math.PI * 3) * 8 + (_rng.NextDouble() - 0.5) * 6;
            else if (progress < 0.92)
                speed = 45 - (progress - 0.80) / 0.12 * 25;
            else
                speed = Math.Max(2, 20 - (progress - 0.92) / 0.08 * 18);

            speed = Math.Max(0, Math.Min(60, speed));
            if (i == 0) speed = 0;
            if (i == routePoints.Length - 1) speed = 0;

            short heading = 0;
            if (i > 0)
            {
                var prev = routePoints[i - 1];
                var dLat = lat - prev.Lat;
                var dLon = (lon - prev.Lon) * Math.Cos(lat * Math.PI / 180);
                heading = (short)((Math.Atan2(dLon, dLat) * 180 / Math.PI + 360) % 360);
            }
            else if (routePoints.Length > 1)
            {
                var next = routePoints[1];
                var dLat = next.Lat - lat;
                var dLon = (next.Lon - lon) * Math.Cos(lat * Math.PI / 180);
                heading = (short)((Math.Atan2(dLon, dLat) * 180 / Math.PI + 360) % 360);
            }

            var satellites = (short)_rng.Next(7, 13);
            var hdop = Math.Round(0.5m + (decimal)_rng.NextDouble() * 0.8m, 1);
            odometer += (decimal)(speed > 0 ? speed / 3600.0 * 5 : 0);

            positions.Add(TGpsPosition.Create(
                vehicle.Id,
                $"SEED-{vehicleIndex + 1}",
                startTime.AddSeconds(i * 5),
                _gf.CreatePoint(new Coordinate(lon, lat)),
                Math.Round((decimal)speed, 1),
                heading,
                true,
                Math.Round(odometer, 1),
                hdop,
                satellites));
        }

        db.GpsPositions.AddRange(positions);

        vehicle.RecordLastPosition(positions.Last().Geometry, positions.Last().RecordedAt);
        db.Vehicles.Update(vehicle);

        await db.SaveChangesAsync();
    }

    private static Polygon CreatePolygon(double lat1, double lon1, double lat2, double lon2)
    {
        var coords = new[]
        {
            new Coordinate(lon1, lat1),
            new Coordinate(lon2, lat1),
            new Coordinate(lon2, lat2),
            new Coordinate(lon1, lat2),
            new Coordinate(lon1, lat1)
        };
        return _gf.CreatePolygon(coords);
    }

    private static LineString Line(Coordinate[] coords) => _gf.CreateLineString(coords);

    private static Coordinate[] ToCoords(params (double Lon, double Lat)[] pts)
        => pts.Select(p => new Coordinate(p.Lon, p.Lat)).ToArray();

    private static GeoCoordinate Dto((double Lon, double Lat) p) => new(p.Lat, p.Lon);

    private static List<GeoCoordinate> RouteACoordinateRoute() => new()
    {
        Dto(MINA), Dto(C_NORTE), Dto(C_CENTRAL), Dto(C_SUR), Dto(C_PLANTA), Dto(PLANTA)
    };

    private static List<GeoCoordinate> RouteBCoordinateRoute() => new()
    {
        Dto(MINA), Dto(S1), Dto(S2), Dto(S3), Dto(S4), Dto(PLANTA)
    };

    private static List<GeoCoordinate> RouteCCoordinateRoute() => new()
    {
        Dto(MINA), Dto(C_NORTE), Dto(N1), Dto(N2), Dto(N3), Dto(N4), Dto(PLANTA)
    };

    private static readonly (double Lon, double Lat) MINA = (-72.8875, -13.6375);
    private static readonly (double Lon, double Lat) C_NORTE = (-72.8790, -13.6300);
    private static readonly (double Lon, double Lat) C_CENTRAL = (-72.8765, -13.6245);
    private static readonly (double Lon, double Lat) C_SUR = (-72.8697, -13.6195);
    private static readonly (double Lon, double Lat) C_PLANTA = (-72.8640, -13.6140);
    private static readonly (double Lon, double Lat) PLANTA = (-72.8575, -13.6075);

    private static readonly (double Lon, double Lat) N1 = (-72.8835, -13.6305);
    private static readonly (double Lon, double Lat) N2 = (-72.8785, -13.6225);
    private static readonly (double Lon, double Lat) N3 = (-72.8725, -13.6165);
    private static readonly (double Lon, double Lat) N4 = (-72.8635, -13.6105);

    private static readonly (double Lon, double Lat) S1 = (-72.8905, -13.6350);
    private static readonly (double Lon, double Lat) S2 = (-72.8910, -13.6290);
    private static readonly (double Lon, double Lat) S3 = (-72.8850, -13.6230);
    private static readonly (double Lon, double Lat) S4 = (-72.8655, -13.6130);
}