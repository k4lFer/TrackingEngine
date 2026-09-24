using System.Globalization;
using System.Text;
using App.Interfaces.Ports.Tracking;
using App.Objects.Gps.DTOs;

namespace App.UseCases.Gps.Decoders;

/// <summary>
/// Decodificador de tramas TK103 en modo GPRS:
///   *HQ,123456789012345,V1,235959,A,1204.6374,S,07704.2793,W,000.0,000,190926,FFFFFBFF#
/// Campos: [0]*HQ [1]deviceId [2]comando [3]hora(hhmmss) [4]estado(A/V)
///         [5]lat(ddmm.mmmm) [6]N/S [7]lon(dddmm.mmmm) [8]E/W
///         [9]velocidad(km/h) [10]curso [11]fecha(ddmmyy) [12]checksum
/// Las coordenadas vienen en grados+minutos (ddmm.mmmm); se convierten a decimal.
/// </summary>
public sealed class Tk103FrameDecoder : IGpsDeviceDecoder
{
    public string Name => "TK103 (GPRS)";

    public bool CanDecode(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < 6) return false;
        // Frames válidos comienzan con *HQ (pueden traer más bytes; la trama se corta al encontrar #).
        return payload[0] == (byte)'*';
    }

    public bool TryDecode(ReadOnlySpan<byte> payload, out IReadOnlyList<DeviceGpsReading> readings)
    {
        readings = [];

        var frame = Encoding.ASCII.GetString(payload);
        // Eliminar marcadores de inicio (*HQ) y fin (#) si vienen incluidos.
        frame = frame.TrimEnd('\r', '\n', '#');
        if (!frame.StartsWith("*HQ", StringComparison.Ordinal)) return false;

        var fields = frame.Split(',');
        if (fields.Length < 10) return false;

        var deviceId = fields[1].TrimStart('+');
        var status = fields[4];

        if (!double.TryParse(fields[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)
            || !double.TryParse(fields[7], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
        {
            return true; // Trama del protocolo pero sin posición útil.
        }

        if (string.IsNullOrEmpty(status) || status is "V" or "v")
        {
            return true; // Sin fix válido.
        }

        var latDir = fields.Length > 6 ? fields[6] : "N";
        var lonDir = fields.Length > 8 ? fields[8] : "E";

        var latitude = ToDecimalDegrees(lat, latDir, isLatitude: true);
        var longitude = ToDecimalDegrees(lon, lonDir, isLatitude: false);

        DateTime? recordedAt = null;
        if (fields.Length > 11 + 1 && TryParseDateTime(fields[11], fields[3], out var ts))
        {
            recordedAt = ts;
        }

        decimal? speed = null;
        if (decimal.TryParse(fields[9], NumberStyles.Float, CultureInfo.InvariantCulture, out var sp))
        {
            speed = sp;
        }

        short? course = null;
        if (short.TryParse(fields[10], NumberStyles.Integer, CultureInfo.InvariantCulture, out var crs))
        {
            course = crs;
        }

        readings =
        [
            new DeviceGpsReading(deviceId, latitude, longitude, recordedAt, speed, course)
        ];
        return true;
    }

    private static double ToDecimalDegrees(double value, string direction, bool isLatitude)
    {
        // ddmm.mmmm / dddmm.mmmm
        var degrees = (int)(value / 100);
        var minutes = value - degrees * 100;
        var result = degrees + minutes / 60.0;
        var negative = isLatitude ? direction == "S" : direction == "W";
        return negative ? -result : result;
    }

    private static bool TryParseDateTime(string date, string time, out DateTime value)
    {
        value = default;
        // fecha dd/mm/yy, hora hh/mm/ss (UTC en estos dispositivos).
        if (date.Length != 6 || time.Length != 6) return false;
        if (!int.TryParse(date[..2], out var day) ||
            !int.TryParse(date[2..4], out var month) ||
            !int.TryParse(date[4..6], out var year) ||
            !int.TryParse(time[..2], out var hour) ||
            !int.TryParse(time[2..4], out var minute) ||
            !int.TryParse(time[4..6], out var second))
        {
            return false;
        }

        try
        {
            value = new DateTime(2000 + year, month, day, hour, minute, second, DateTimeKind.Utc);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}