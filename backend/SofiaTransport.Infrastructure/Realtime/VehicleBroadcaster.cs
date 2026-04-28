using Microsoft.AspNetCore.SignalR;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Infrastructure.Realtime;

public interface IVehicleBroadcaster
{
    Task BroadcastAsync(Vehicle vehicle);
}

public class VehicleBroadcaster : IVehicleBroadcaster
{
    private readonly IHubContext<VehicleHub> _hub;

    public VehicleBroadcaster(IHubContext<VehicleHub> hub) => _hub = hub;

    public async Task BroadcastAsync(Vehicle vehicle)
    {
        var payload = new
        {
            vehicleId = vehicle.VehicleId,
            routeId = vehicle.RouteId,
            tripId = vehicle.TripId,
            lat = vehicle.Lat,
            lon = vehicle.Lon,
            vehicle.Bearing,
            vehicle.Speed,
            vehicle.RecordedAt
        };

        if (!string.IsNullOrEmpty(vehicle.RouteId))
        {
            await _hub.Clients.Group($"route:{vehicle.RouteId}").SendAsync("VehicleUpdated", payload);
            await _hub.Clients.Group("all").SendAsync("VehicleUpdated", payload);
        }
    }
}
