using App.Shared.Common.Enums;

namespace App.UseCases.Tracking;

/// <summary>
/// Traduce una trama GPS a un estado de vehículo.
///
/// La velocidad sola no basta. Un camión cargado 25 minutos en una zona de carga
/// está a 0 km/h, pero no está "detenido": está trabajando. Marcarlo como Detenido
/// cuenta una duración de parada que nadie pidió y, al pasar los 5 minutos, dispara
/// una alarma de parada larga sobre un vehículo que está haciendo su trabajo.
///
/// La regla: la velocidad manda sobre el movimiento, y el tipo de zona manda sobre
/// la etiqueta cuando el vehículo está quieto dentro de una zona operacional.
/// </summary>
public static class GpsStateResolver
{
    /// <summary>
    /// Resuelve el estado. <paramref name="zoneKind"/> es el tipo de la geocerca
    /// que contiene el punto, o null si el vehículo está fuera de cualquier zona.
    /// </summary>
    public static VehicleState Resolve(decimal? speedKmh, GeofenceKind? zoneKind, decimal stopSpeedKmh)
    {
        var speed = speedKmh ?? 0;

        // Moviéndose: el tipo de zona da igual. Un camión cruzando una zona de
        // carga a 40 km/h no está cargando.
        if (speed >= stopSpeedKmh)
        {
            return VehicleState.EnRuta;
        }

        if (zoneKind is null)
        {
            return VehicleState.Detenido;
        }

        return zoneKind.Value switch
        {
            GeofenceKind.Load => VehicleState.EnCarga,
            GeofenceKind.Unload => VehicleState.EnDescarga,
            GeofenceKind.Wait => VehicleState.Disponible,
            _ => VehicleState.Detenido,
        };
    }
}
