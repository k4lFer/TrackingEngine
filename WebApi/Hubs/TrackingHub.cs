using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs;

/// <summary>Hub SignalR para el tablero en vivo del motor de seguimiento.</summary>
[AllowAnonymous]
public class TrackingHub : Hub
{
    public async Task SubscribeVehicle(Guid vehicleId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"vehicle-{vehicleId:N}");

    public async Task UnsubscribeVehicle(Guid vehicleId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"vehicle-{vehicleId:N}");

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", new
        {
            message = "Conectado al motor en vivo",
            serverTime = DateTime.UtcNow
        });
        await base.OnConnectedAsync();
    }
}