using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SofiaTransport.Infrastructure.Realtime;

[Authorize]
public class VehicleHub : Hub
{
    public const string HubPath = "/hubs/vehicles";

    public async Task SubscribeToRoute(string routeId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"route:{routeId}");

    public async Task UnsubscribeFromRoute(string routeId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"route:{routeId}");

    public async Task SubscribeToAlerts()
        => await Groups.AddToGroupAsync(Context.ConnectionId, "alerts");

    public async Task UnsubscribeFromAlerts()
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, "alerts");
}