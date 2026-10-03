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

    /// <summary>
    /// Velocidad por debajo de la cual la trama se considera "Detenido". Debe
    /// coincidir con el umbral del ReportPositionCommandHandler; ambos leen de aquí.
    /// </summary>
    public decimal StopSpeedKmh { get; set; } = 3;

    /// <summary>
    /// Distancia mínima desde el último punto persistido para guardar una nueva
    /// posición si el vehículo no está detenido. 30 m ≈ señal GPS típica en parado.
    /// Ponerlo en 0 desactiva el filtro por distancia.
    /// </summary>
    public double MinDistanceMeters { get; set; } = 30;

    /// <summary>
    /// Tope de supresión en un mismo punto. Aunque el vehículo no se mueva, se
    /// guarda al menos una posición cada N segundos, para que la traza siga viva.
    /// </summary>
    public int MaxSuppressSeconds { get; set; } = 120;

    /// <summary>
    /// Cada cuántos segundos, como máximo, se refresca "última señal" de un
    /// vehículo cuya trama fue suprimida.
    /// </summary>
    public int HeartbeatSeconds { get; set; } = 30;

    /// <summary>
    /// Tope de vehículos rastreados en memoria por el filtro. Es una salvaguarda
    /// para que un flood de deviceIds desconocidos no crezca el diccionario sin fin.
    /// </summary>
    public int MaxTrackedVehicles { get; set; } = 5000;

    /// <summary>
    /// Segundos sin recibir tramas antes de marcar el vehículo como Offline.
    ///
    /// Con 300 s y el latido a 30 s, un vehículo sano nunca se acerca al umbral
    /// (peor caso 30 s, margen de 10x). Es deliberadamente holgado: en mina un
    /// vehículo puede perder cobertura en un túnel o bajo una pila de mineral
    /// sin estar apagado, y un umbral corto convertiría eso en una avalancha de
    /// alarmas falsas.
    /// </summary>
    public int OfflineThresholdSeconds { get; set; } = 300;

    /// <summary>Cada cuánto tiempo el worker revisa si hay vehículos sin señal.</summary>
    public int StaleScanIntervalSeconds { get; set; } = 60;
}
