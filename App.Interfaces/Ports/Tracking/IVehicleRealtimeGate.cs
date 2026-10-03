using App.Shared.Common.Enums;

namespace App.Interfaces.Ports.Tracking;

/// <summary>
/// Filtro de tramas GPS repetidas. Decide si una trama aporta información nueva
/// o si es ruido de un vehículo detenido (que a 1 Hz genera 86 400 tramas/día).
///
/// Debe registrarse como SINGLETON: si su estado viviera en un servicio Scoped,
/// el gateway TCP —que crea un scope por trama— lo reiniciaría en cada envío y
/// el filtro no filtraría nada.
///
/// La implementación inicial es en memoria. Si el backend pasa a correr en más de
/// una instancia, este puerto es el punto de sustitución por Redis.
/// </summary>
public interface IVehicleRealtimeGate
{
    /// <summary>
    /// Evalúa la trama y deja constancia de ella para la siguiente.
    /// <paramref name="nowUtc"/> debe venir del reloj del servidor, no del dispositivo:
    /// un teléfono con la hora desfasada no debe poder abrir la ventana de supresión.
    /// </summary>
    GpsGateDecision Evaluate(Guid vehicleId, double latitude, double longitude, decimal? speedKmh, DateTime nowUtc);

    /// <summary>
    /// Reserva el latido de "última señal" de un vehículo. Devuelve true solo una vez
    /// por ventana de <c>GpsRealtimeOptions.HeartbeatSeconds</c>.
    ///
    /// Vive aquí, y no en el servicio de ingesta, por lo mismo que el resto del
    /// estado del filtro: la ingesta es Scoped y el gateway TCP crea un scope por
    /// trama, así que un contador guardado allí se reiniciaría en cada envío y
    /// refrescaría la señal en todas.
    /// </summary>
    bool TryClaimHeartbeat(Guid vehicleId, DateTime nowUtc);

    /// <summary>
    /// Reserva la ventana de refresco global para los clientes. Es global y no por
    /// vehículo a propósito: cada DataChangedAsync hace que el frontend re-consulte
    /// sus queries, y con 50 vehículos latiendo a la vez serían 50 refetches completos
    /// en la misma ventana.
    /// </summary>
    bool TryClaimHeartbeatBroadcast(DateTime nowUtc);
}
