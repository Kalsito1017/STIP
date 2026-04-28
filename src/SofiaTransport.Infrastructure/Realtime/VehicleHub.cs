using Microsoft.AspNetCore.SignalR;

namespace SofiaTransport.Infrastructure.Realtime;

public class VehicleHub : Hub
{
    public const string HubPath = "/hubs/vehicles";

    public async Task SubscribeToRoute(string routeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"route:{routeId}");
    }

    public async Task UnsubscribeFromRoute(string routeId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"route:{routeId}");
    }
}
