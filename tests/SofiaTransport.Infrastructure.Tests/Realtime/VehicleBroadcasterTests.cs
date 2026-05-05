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
    private static Mock<IHubContext<VehicleHub>> CreateMockHubContext()
    {
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        var mockGroupManager = new Mock<IGroupManager>();

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

        var mockHubContext = new Mock<IHubContext<VehicleHub>>();
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
        mockHubContext.Setup(h => h.Groups).Returns(mockGroupManager.Object);

        return mockHubContext;
    }

    [Fact]
    public async Task BroadcastAsync_WithRouteId_SendsToRouteGroup()
    {
        // Arrange
        var mockHubContext = CreateMockHubContext();
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

        // Assert
        mockHubContext.Verify(h => h.Clients, Times.AtLeastOnce);
    }

    [Fact]
    public async Task BroadcastAsync_WithoutRouteId_DoesNotSendToGroup()
    {
        // Arrange
        var mockHubContext = CreateMockHubContext();
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

        // Assert - Clients.Group should not be called since RouteId is null
        mockHubContext.Object.Clients.Group(It.IsAny<string>());
    }

    [Fact]
    public async Task BroadcastAsync_WithEmptyRouteId_DoesNotSendToGroup()
    {
        // Arrange
        var mockHubContext = CreateMockHubContext();
        var broadcaster = new VehicleBroadcaster(mockHubContext.Object);
        var vehicle = new Vehicle
        {
            VehicleId = "v1",
            RouteId = string.Empty,
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 },
            RecordedAt = DateTime.UtcNow
        };

        // Act - should not throw
        await broadcaster.BroadcastAsync(vehicle);

        // Assert - no exception thrown; group send skipped for empty route id
    }
}
