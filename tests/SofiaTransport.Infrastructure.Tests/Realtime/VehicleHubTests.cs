using Microsoft.AspNetCore.SignalR;
using Moq;
using SofiaTransport.Infrastructure.Realtime;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.Realtime;

public class VehicleHubTests
{
    private static (VehicleHub hub, Mock<HubCallerContext> mockContext, Mock<IGroupManager> mockGroups) CreateHub()
    {
        var mockGroups = new Mock<IGroupManager>();
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.ConnectionId).Returns("conn-123");

        var hub = new VehicleHub
        {
            Context = mockContext.Object,
            Groups = mockGroups.Object
        };

        return (hub, mockContext, mockGroups);
    }

    [Fact]
    public async Task SubscribeToRoute_AddsToRouteGroup()
    {
        // Arrange
        var (hub, _, mockGroups) = CreateHub();

        // Act
        await hub.SubscribeToRoute("r-1");

        // Assert
        mockGroups.Verify(g => g.AddToGroupAsync("conn-123", "route:r-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnsubscribeFromRoute_RemovesFromRouteGroup()
    {
        // Arrange
        var (hub, _, mockGroups) = CreateHub();

        // Act
        await hub.UnsubscribeFromRoute("r-204");

        // Assert
        mockGroups.Verify(g => g.RemoveFromGroupAsync("conn-123", "route:r-204", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubscribeToAlerts_AddsToAlertsGroup()
    {
        // Arrange
        var (hub, _, mockGroups) = CreateHub();

        // Act
        await hub.SubscribeToAlerts();

        // Assert
        mockGroups.Verify(g => g.AddToGroupAsync("conn-123", "alerts", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnsubscribeFromAlerts_RemovesFromAlertsGroup()
    {
        // Arrange
        var (hub, _, mockGroups) = CreateHub();

        // Act
        await hub.UnsubscribeFromAlerts();

        // Assert
        mockGroups.Verify(g => g.RemoveFromGroupAsync("conn-123", "alerts", It.IsAny<CancellationToken>()), Times.Once);
    }
}
