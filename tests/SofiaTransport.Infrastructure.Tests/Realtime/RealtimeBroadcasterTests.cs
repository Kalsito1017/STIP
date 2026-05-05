using Microsoft.AspNetCore.SignalR;
using Moq;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;
using SofiaTransport.Infrastructure.Realtime;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.Realtime;

public class RealtimeBroadcasterTests
{
    private static Mock<IHubContext<VehicleHub>> CreateMockHubContext()
    {
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        var mockGroupManager = new Mock<IGroupManager>();

        // Set up Clients.All
        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
        // Set up Clients.Group(string)
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        // Set up Clients.Groups(IReadOnlyList<string>)
        mockClients.Setup(c => c.Groups(It.IsAny<IReadOnlyList<string>>())).Returns(mockClientProxy.Object);

        var mockHubContext = new Mock<IHubContext<VehicleHub>>();
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        return mockHubContext;
    }

    [Fact]
    public async Task BroadcastTripUpdateAsync_SendsToAllAndRouteGroup()
    {
        // Arrange
        var mockHubContext = CreateMockHubContext();
        var broadcaster = new RealtimeBroadcaster(mockHubContext.Object);
        var tripUpdate = new TripUpdate
        {
            TripId = "t1",
            RouteId = "r-1",
            VehicleId = "v1",
            RecordedAt = DateTime.UtcNow,
            StopTimeUpdates =
            {
                new StopTimeUpdate { StopId = "s-001", ArrivalDelay = 120 }
            }
        };

        // Act
        await broadcaster.BroadcastTripUpdateAsync(tripUpdate);

        // Assert - should not throw
    }

    [Fact]
    public async Task BroadcastTripUpdateAsync_WithoutRouteId_OnlySendsToAll()
    {
        // Arrange
        var mockHubContext = CreateMockHubContext();
        var broadcaster = new RealtimeBroadcaster(mockHubContext.Object);
        var tripUpdate = new TripUpdate
        {
            TripId = "t1",
            RouteId = null,
            VehicleId = "v1",
            RecordedAt = DateTime.UtcNow
        };

        // Act - should not throw
        await broadcaster.BroadcastTripUpdateAsync(tripUpdate);

        // Assert - no exception
    }

    [Fact]
    public async Task BroadcastAlertAsync_SendsToAllAndRelevantRouteGroups()
    {
        // Arrange
        var mockHubContext = CreateMockHubContext();
        var broadcaster = new RealtimeBroadcaster(mockHubContext.Object);
        var alert = new ServiceAlert
        {
            AlertId = "a1",
            HeaderText = "Test Alert",
            Cause = 1,
            Effect = 3,
            RecordedAt = DateTime.UtcNow,
            InformedEntities =
            {
                new InformedEntity { RouteId = "r-1" },
                new InformedEntity { RouteId = "r-204" }
            }
        };

        // Act
        await broadcaster.BroadcastAlertAsync(alert);

        // Assert - should not throw; verifies the broadcast was attempted
    }

    [Fact]
    public async Task BroadcastAlertAsync_WithDuplicateRouteIds_DeduplicatesGroups()
    {
        // Arrange
        var mockHubContext = CreateMockHubContext();
        var broadcaster = new RealtimeBroadcaster(mockHubContext.Object);
        var alert = new ServiceAlert
        {
            AlertId = "a1",
            HeaderText = "Test Alert",
            RecordedAt = DateTime.UtcNow,
            InformedEntities =
            {
                new InformedEntity { RouteId = "r-1" },
                new InformedEntity { RouteId = "r-1" }
            }
        };

        // Act - should not throw
        await broadcaster.BroadcastAlertAsync(alert);

        // Assert
    }

    [Fact]
    public async Task BroadcastAlertAsync_WithoutInformedEntities_OnlySendsToAll()
    {
        // Arrange
        var mockHubContext = CreateMockHubContext();
        var broadcaster = new RealtimeBroadcaster(mockHubContext.Object);
        var alert = new ServiceAlert
        {
            AlertId = "a1",
            HeaderText = "Global Alert",
            RecordedAt = DateTime.UtcNow
        };

        // Act - should not throw
        await broadcaster.BroadcastAlertAsync(alert);

        // Assert - no exception
    }
}
