using Microsoft.AspNetCore.SignalR;
using Moq;
using NetTopologySuite.Geometries;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;
using SofiaTransport.Infrastructure.Realtime;
using Xunit;
using Coordinates = SofiaTransport.Domain.ValueObjects.Coordinates;

namespace SofiaTransport.Infrastructure.Tests.Realtime;

public class VehicleBroadcasterTests
{
    [Fact]
    public async Task BroadcastAsync_SendsVehicleUpdatedToAllClients()
    {
        // Arrange
        var mockAllClients = new Mock<IClientProxy>();
        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(c => c.All).Returns(mockAllClients.Object);

        var mockHubContext = new Mock<IHubContext<VehicleHub>>();
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        var broadcaster = new VehicleBroadcaster(mockHubContext.Object);
        var vehicle = new Vehicle
        {
            VehicleId = "v1",
            RouteId = "r-1",
            TripId = "t1",
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 },
            Bearing = 90f,
            Speed = 40f,
            RecordedAt = DateTime.UtcNow
        };

        // Act
        await broadcaster.BroadcastAsync(vehicle);

        // Assert - verify Clients.All was used (not a route-specific group)
        mockClients.Verify(c => c.All, Times.Once);
    }

    [Fact]
    public async Task BroadcastAsync_WithoutRouteId_SendsToAllClients()
    {
        // Arrange
        var mockAllClients = new Mock<IClientProxy>();
        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(c => c.All).Returns(mockAllClients.Object);

        var mockHubContext = new Mock<IHubContext<VehicleHub>>();
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        var broadcaster = new VehicleBroadcaster(mockHubContext.Object);
        var vehicle = new Vehicle
        {
            VehicleId = "v1",
            RouteId = null,
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 },
            RecordedAt = DateTime.UtcNow
        };

        // Act
        await broadcaster.BroadcastAsync(vehicle);

        // Assert - vehicles without RouteId are now broadcast to all clients
        mockClients.Verify(c => c.All, Times.Once);
    }

    [Fact]
    public async Task BroadcastAsync_WithEmptyRouteId_SendsToAllClients()
    {
        // Arrange
        var mockAllClients = new Mock<IClientProxy>();
        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(c => c.All).Returns(mockAllClients.Object);

        var mockHubContext = new Mock<IHubContext<VehicleHub>>();
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        var broadcaster = new VehicleBroadcaster(mockHubContext.Object);
        var vehicle = new Vehicle
        {
            VehicleId = "v1",
            RouteId = string.Empty,
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 },
            RecordedAt = DateTime.UtcNow
        };

        // Act
        await broadcaster.BroadcastAsync(vehicle);

        // Assert - vehicles with empty RouteId are now broadcast to all clients
        mockClients.Verify(c => c.All, Times.Once);
    }
}
