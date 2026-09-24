namespace App.Objects.Gps.DTOs;

/// <summary>
/// Lectura GPS normalizada, resultado de decodificar un frame/protocolo de un
/// dispositivo. Es el punto común entre los distintos protocolos y el pipeline
/// de tracking (ReportPositionCommand).
/// </summary>
public sealed record DeviceGpsReading(
    string DeviceId,
    double Latitude,
    double Longitude,
    DateTime? RecordedAt = null,
    decimal? SpeedKmh = null,
    short? HeadingDeg = null,
    decimal? Hdop = null,
    short? Satellites = null,
    bool? Ignition = null
);