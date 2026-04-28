using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Stops;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Application.Tests.Stops;

public class GetNearbyStopsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsNearbyStops()
    {
        // Arrange
        var stops = new List<Stop>
        {
            new() { StopId = "s-001", StopName = "Central Station", Location = new Coordinates(42.6977, 23.3219) },
            new() { StopId = "s-002", StopName = "Vitosha Blvd", Location = new Coordinates(42.6939, 23.3451) }
        };

        var mockRepo = new Mock<IStopRepository>();
        mockRepo.Setup(r => r.GetNearbyAsync(42.6977, 23.3219, 1.0)).ReturnsAsync(stops);

        var handler = new GetNearbyStopsHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(new GetNearbyStopsQuery(42.6977, 23.3219, 1.0), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.StopId == "s-001");
        Assert.Contains(result, s => s.StopId == "s-002");
    }

    [Fact]
    public async Task Handle_NoNearbyStops_ReturnsEmptyList()
    {
        // Arrange
        var mockRepo = new Mock<IStopRepository>();
        mockRepo.Setup(r => r.GetNearbyAsync(42.7, 23.3, 0.1)).ReturnsAsync(Array.Empty<Stop>());

        var handler = new GetNearbyStopsHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(new GetNearbyStopsQuery(42.7, 23.3, 0.1), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
