using System.Globalization;
using App.Interfaces.Ports.Tracking;
using App.Objects.Gps.DTOs;

namespace App.UseCases.Gps.Decoders;

/// <summary>
/// Decodificador del protocolo OsmAnd (usado por Traccar Client y OsmAnd).
/// Los reportes llegan como query string: id/deviceid, lat, lon o location,
/// timestamp, valid, speed (nudos por defecto), bearing/heading, hdop.
/// </summary>
public sealed class OsmAndQueryDecoder : IGpsDeviceDecoder
{
    public const double KnotsToKmh = 1.852;

    public string Name => "OsmAnd (query string)";

    public bool CanDecode(ReadOnlySpan<byte> payload)
    {
        // Trabaja sobre texto; no debe robarse tramas binarias de otros protocolos.
        return ContainsAscii(payload, '=') && ContainsAscii(payload, '&');
    }

    public bool TryDecode(ReadOnlySpan<byte> payload, out IReadOnlyList<DeviceGpsReading> readings)
    {
        readings = [];
        var text = System.Text.Encoding.UTF8.GetString(payload);

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in text.Split(['&'], StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = pair.IndexOf('=');
            if (idx <= 0) continue;
            parameters[pair[..idx]] = System.Uri.UnescapeDataString(pair[(idx + 1)..]);
        }

        var deviceId = GetKey(parameters, "id", "deviceid");
        if (string.IsNullOrWhiteSpace(deviceId)) return false;

        // valid=false se ignora (sin fix).
        var valid = GetKey(parameters, "valid");
        if (valid is not null && valid is "false" or "0") return true;

        double lat, lon;
        var location = GetKey(parameters, "location");
        if (location is not null)
        {
            var parts = location.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 2 || !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lat) ||
                !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
            {
                return false;
            }
        }
        else
        {
            var latStr = GetKey(parameters, "lat");
            var lonStr = GetKey(parameters, "lon");
            if (latStr is null || lonStr is null ||
                !double.TryParse(latStr, NumberStyles.Float, CultureInfo.InvariantCulture, out lat) ||
                !double.TryParse(lonStr, NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
            {
                return false;
            }
        }

        DateTime? recordedAt = null;
        var timestamp = GetKey(parameters, "timestamp");
        if (timestamp is not null && TryParseTimestamp(timestamp, out var ts)) recordedAt = ts;

        decimal? speedKmh = null;
        var speed = GetKey(parameters, "speed");
        if (speed is not null && decimal.TryParse(speed, NumberStyles.Float, CultureInfo.InvariantCulture, out var knots))
        {
            speedKmh = Math.Round(knots * (decimal)KnotsToKmh, 1);
        }

        short? heading = null;
        var headingStr = GetKey(parameters, "heading", "bearing");
        if (headingStr is not null && short.TryParse(headingStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var h))
        {
            heading = h;
        }

        decimal? hdop = null;
        var hdopStr = GetKey(parameters, "hdop");
        if (hdopStr is not null && decimal.TryParse(hdopStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var hd))
        {
            hdop = hd;
        }

        readings =
        [
            new DeviceGpsReading(deviceId, lat, lon, recordedAt, speedKmh, heading, hdop)
        ];
        return true;
    }

    private static string? GetKey(IReadOnlyDictionary<string, string> parameters, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (parameters.TryGetValue(key, out var value)) return value;
        }
        return null;
    }

    private static bool TryParseTimestamp(string raw, out DateTime value)
    {
        value = default;
        if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var epoch))
        {
            value = epoch < 10000000000L
                ? DateTimeOffset.FromUnixTimeSeconds(epoch).UtcDateTime
                : DateTimeOffset.FromUnixTimeMilliseconds(epoch).UtcDateTime;
            return true;
        }
        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out value))
        {
            return true;
        }
        return false;
    }

    private static bool ContainsAscii(ReadOnlySpan<byte> bytes, char c)
    {
        foreach (var b in bytes)
        {
            if (b == (byte)c) return true;
        }
        return false;
    }
}