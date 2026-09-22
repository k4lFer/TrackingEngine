using App.Objects.Simulation.DTOs.Output.Response;

namespace App.Interfaces.Ports.Tracking;

/// <summary>
/// Notifica a los clientes SignalR (dashboard) los cambios en vivo del motor:
/// movimiento de vehiculos, cambios de viaje y refresco global de datos.
/// </summary>
public interface ITrackingNotifier
{
    Task VehicleMovedAsync(VehicleLiveDto dto, CancellationToken cancellationToken = default);
    Task TripChangedAsync(TripLiveDto dto, CancellationToken cancellationToken = default);
    Task DataChangedAsync(CancellationToken cancellationToken = default);
}