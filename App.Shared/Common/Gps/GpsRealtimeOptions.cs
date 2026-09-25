namespace App.Shared.Common.Gps;

/// <summary>
/// Umbrales de la ingesta GPS en tiempo real. Sección de configuración: "GpsRealtime".
/// </summary>
public sealed class GpsRealtimeOptions
{
    /// <summary>
    /// Segundos detenido antes de registrar una parada larga (LongStopDetected).
    /// </summary>
    public int LongStopThresholdSeconds { get; set; } = 300;
}
