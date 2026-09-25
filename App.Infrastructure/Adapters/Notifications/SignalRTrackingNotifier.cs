using App.Interfaces.Ports.Tracking;
using App.Objects.Simulation.DTOs.Output.Response;
using Microsoft.AspNetCore.SignalR;

namespace App.Infrastructure.Adapters.Notifications;

/// <summary>
/// Notificador SignalR del motor: emite el movimiento de vehículos, los cambios
/// de operación y el refresco global de datos hacia el tablero.
/// </summary>
public sealed class SignalRTrackingNotifier : ITrackingNotifier
{
    private readonly IHubContext<TrackingHub> _hub;

    public SignalRTrackingNotifier(IHubContext<TrackingHub> hub)
    {
        _hub = hub;
    }

    public async Task VehicleMovedAsync(VehicleLiveDto dto, CancellationToken cancellationToken = default)
    {
        await _hub.Clients.Group($"vehicle-{dto.VehicleId:N}").SendAsync("VehicleMoved", dto, cancellationToken);
        await _hub.Clients.Group("all").SendAsync("VehicleMoved", dto, cancellationToken);
    }

    public async Task TripChangedAsync(TripLiveDto dto, CancellationToken cancellationToken = default)
    {
        await _hub.Clients.Group($"vehicle-{dto.VehicleId:N}").SendAsync("TripChanged", dto, cancellationToken);
        await _hub.Clients.Group("all").SendAsync("TripChanged", dto, cancellationToken);
    }

    public Task DataChangedAsync(CancellationToken cancellationToken = default)
        => _hub.Clients.All.SendAsync("DataChanged", DateTime.UtcNow, cancellationToken);
}