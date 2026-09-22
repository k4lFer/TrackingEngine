using System.Net.Http.Json;
using System.Text.Json.Nodes;
using App.Interfaces.Ports.Routes;
using App.Shared.Geometry;
using App.Shared.Objects.Enums;
using App.Shared.Routing;

namespace App.Infrastructure.Adapters.Routes;

/// <summary>
/// Cliente de Valhalla (/route). Devuelve la geometría en GeoJSON de la ruta
/// por la red vial real usando el perfil de camión. Cualquier fallo devuelve
/// null para que el caller use el fallback de red interna de la mina.
/// </summary>
public sealed class ValhallaRoutePlanner : IRoutePlanner
{
    private readonly HttpClient _http;
    private readonly ValhallaOptions _options;

    public ValhallaRoutePlanner(HttpClient http, ValhallaOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<RoutePlan?> PlanAsync(
        RoutingProfile profile,
        GeoCoordinate from,
        GeoCoordinate to,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                locations = new[]
                {
                    new { lon = from.Lon, lat = from.Lat },
                    new { lon = to.Lon, lat = to.Lat }
                },
                costing = ProfileName(profile),
                alternates = 2,
                directions_options = new
                {
                    units = "kilometers",
                    language = "es-ES",
                    shape_format = "polyline6"
                }
            };

            using var response = await _http.PostAsJsonAsync("route", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken);
            if (json is null)
                return null;

            var plan = ParseTrip(json["trip"]);
            if (plan is null)
                return null;

            return plan with { Alternatives = DecodeAlternates(json["alternates"]) };
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Lee un elemento <c>trip</c> (o trip de una alternativa) y devuelve la
    /// geometría decodificada, el resumen (longitud/tiempo), las maneuvers paso a
    /// paso y el shape crudo en Polyline6 tal cual lo entrega Valhalla.
    /// </summary>
    private static RoutePlan? ParseTrip(JsonNode? trip)
    {
        if (trip is not JsonObject tripObj)
            return null;

        var legs = tripObj["legs"];
        if (legs is not JsonArray { Count: > 0 } legList)
            return null;

        var summary = tripObj["summary"];
        var points = DecodeShape(legList[0]?["shape"]);
        if (points is null || points.Count < 2)
            return null;

        return new RoutePlan(
            points,
            DistanceMeters(summary?["length"]),
            DurationSeconds(summary?["time"]))
        {
            Maneuvers = DecodeManeuvers(legList[0]?["maneuvers"]),
            RawShape = RawShapeOf(legList[0]?["shape"])
        };
    }

    private static List<GeoCoordinate>? DecodeShape(JsonNode? shape) =>
        shape switch
        {
            JsonArray pairList => DecodeGeoJson(pairList),
            JsonValue { } encoded when TryDecodePolyline6(encoded.ToString(), out var decoded) => decoded,
            _ => null
        };

    private static List<RoutePlan> DecodeAlternates(JsonNode? alternates)
    {
        var result = new List<RoutePlan>();
        if (alternates is not JsonArray altArray)
            return result;

        foreach (var alt in altArray)
        {
            var trip = ParseTrip(alt?["trip"]);
            if (trip is not null)
                result.Add(trip);
        }
        return result;
    }

    private static IReadOnlyList<RouteManeuver>? DecodeManeuvers(JsonNode? maneuversNode)
    {
        if (maneuversNode is not JsonArray array)
            return null;

        var list = new List<RouteManeuver>(array.Count);
        foreach (var item in array)
        {
            if (item is not JsonObject maneuver)
                continue;

            list.Add(new RouteManeuver(
                maneuver["type"]?.GetValue<int>() ?? 0,
                maneuver["instruction"]?.GetValue<string>(),
                StreetNames(maneuver["street_names"]),
                maneuver["length"]?.GetValue<double>() ?? 0.0,
                maneuver["time"]?.GetValue<double>() ?? 0.0));
        }
        return list;
    }

    private static IReadOnlyList<string> StreetNames(JsonNode? node)
    {
        if (node is not JsonArray array)
            return Array.Empty<string>();

        var names = new List<string>(array.Count);
        foreach (var item in array)
        {
            if (item?.GetValue<string>() is string name)
                names.Add(name);
        }
        return names;
    }

    private static string? RawShapeOf(JsonNode? shape) =>
        shape switch
        {
            JsonValue value when value.TryGetValue(out string encoded) => encoded,
            _ => null
        };

    private static List<GeoCoordinate>? DecodeGeoJson(JsonArray pairList)
    {
        var points = new List<GeoCoordinate>(pairList.Count);
        foreach (var item in pairList)
        {
            if (item is not JsonArray pair || pair.Count < 2 ||
                pair[0]?.GetValue<double>() is not double lon ||
                pair[1]?.GetValue<double>() is not double lat ||
                !double.IsFinite(lon) || !double.IsFinite(lat))
            {
                return null;
            }

            points.Add(new GeoCoordinate(lat, lon));
        }
        return points;
    }

    private static bool TryDecodePolyline6(string encoded, out List<GeoCoordinate> points)
    {
        points = new List<GeoCoordinate>();
        int index = 0;
        int len = encoded.Length;
        int latAcc = 0;
        int lonAcc = 0;
        while (index < len)
        {
            if (!TryDecodeValue(encoded, ref index, out int latDelta) ||
                !TryDecodeValue(encoded, ref index, out int lonDelta))
            {
                return false;
            }

            latAcc += latDelta;
            lonAcc += lonDelta;
            points.Add(new GeoCoordinate(latAcc / 1e6, lonAcc / 1e6));
        }
        return points.Count >= 2;
    }

    private static bool TryDecodeValue(string source, ref int index, out int delta)
    {
        delta = 0;
        int shift = 0;
        int current;
        do
        {
            if (index >= source.Length || shift > 60)
                return false;
            current = source[index++] - 63;
            if (current < 0 || current > 63)
                return false;
            delta |= (current & 0x1f) << shift;
            shift += 5;
        } while (current >= 0x20);

        delta = (delta & 1) != 0 ? ~(delta >> 1) : (delta >> 1);
        return true;
    }

    private static double DistanceMeters(JsonNode? lengthKm) =>
        lengthKm?.GetValue<double>() is double km && double.IsFinite(km) ? km * 1000.0 : 0.0;

    private static double DurationSeconds(JsonNode? timeS) =>
        timeS?.GetValue<double>() is double s && double.IsFinite(s) ? s : 0.0;

    private static string ProfileName(RoutingProfile profile) =>
        profile == RoutingProfile.Truck ? "truck" : "auto";
}