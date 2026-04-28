using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Analytics;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Application.Tests.Analytics;

public class GetDelayHeatmapHandlerTests
{
    [Fact]
    public async Task Handle_WithLogs_ReturnsHeatmapPoints()
    {
        // Arrange
        var from = new DateTime(2026, 4, 20);
        var to = new DateTime(2026, 4, 27);

        var logs = new List<DelayLog>
        {
            new() { StopId = "s-001", DelaySeconds = 60 },
            new() { StopId = "s-001", DelaySeconds = 120 },
            new() { StopId = "s-002", DelaySeconds = 30 },
        };

        var stops = new List<Stop>
        {
            new() { StopId = "s-001", StopName = "Orlov Most", Location = new Coordinates(42.6897, 23.3342) },
            new() { StopId = "s-002", StopName = "NDK", Location = new Coordinates(42.6871, 23.3186) },
        };

        var mockDelayRepo = new Mock<IDelayLogRepository>();
        mockDelayRepo.Setup(r => r.GetForHeatmapAsync(from, to)).ReturnsAsync(logs);

        var mockStopRepo = new Mock<IStopRepository>();
        mockStopRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(stops);

        var handler = new GetDelayHeatmapHandler(mockDelayRepo.Object, mockStopRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetDelayHeatmapQuery(from, to), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);

        var point1 = result.First(p => p.Lat == 42.6897 && p.Lon == 23.3342);
        Assert.Equal(90.0, point1.AvgDelaySeconds); // (60+120)/2
        Assert.Equal(2, point1.SampleCount);

        var point2 = result.First(p => p.Lat == 42.6871 && p.Lon == 23.3186);
        Assert.Equal(30.0, point2.AvgDelaySeconds);
        Assert.Equal(1, point2.SampleCount);
    }

    [Fact]
    public async Task Handle_NoLogs_ReturnsEmptyList()
    {
        // Arrange
        var from = new DateTime(2026, 4, 20);
        var to = new DateTime(2026, 4, 27);

        var mockDelayRepo = new Mock<IDelayLogRepository>();
        mockDelayRepo.Setup(r => r.GetForHeatmapAsync(from, to))
            .ReturnsAsync(Array.Empty<DelayLog>());

        var mockStopRepo = new Mock<IStopRepository>();
        mockStopRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Stop>());

        var handler = new GetDelayHeatmapHandler(mockDelayRepo.Object, mockStopRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetDelayHeatmapQuery(from, to), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_LogsWithUnknownStop_FilteredOut()
    {
        // Arrange
        var from = new DateTime(2026, 4, 20);
        var to = new DateTime(2026, 4, 27);

        var logs = new List<DelayLog>
        {
            new() { StopId = "s-001", DelaySeconds = 60 },
            new() { StopId = "s-unknown", DelaySeconds = 999 }, // stop doesn't exist
        };

        var stops = new List<Stop>
        {
            new() { StopId = "s-001", StopName = "Orlov Most", Location = new Coordinates(42.6897, 23.3342) },
        };

        var mockDelayRepo = new Mock<IDelayLogRepository>();
        mockDelayRepo.Setup(r => r.GetForHeatmapAsync(from, to)).ReturnsAsync(logs);

        var mockStopRepo = new Mock<IStopRepository>();
        mockStopRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(stops);

        var handler = new GetDelayHeatmapHandler(mockDelayRepo.Object, mockStopRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetDelayHeatmapQuery(from, to), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(42.6897, result[0].Lat);
        Assert.Equal(60.0, result[0].AvgDelaySeconds);
    }

    [Fact]
    public async Task Handle_LogsWithNullStopId_FilteredOut()
    {
        // Arrange
        var from = new DateTime(2026, 4, 20);
        var to = new DateTime(2026, 4, 27);

        var logs = new List<DelayLog>
        {
            new() { StopId = null, DelaySeconds = 999 },
            new() { StopId = "s-001", DelaySeconds = 30 },
        };

        var stops = new List<Stop>
        {
            new() { StopId = "s-001", StopName = "Orlov Most", Location = new Coordinates(42.6897, 23.3342) },
        };

        var mockDelayRepo = new Mock<IDelayLogRepository>();
        mockDelayRepo.Setup(r => r.GetForHeatmapAsync(from, to)).ReturnsAsync(logs);

        var mockStopRepo = new Mock<IStopRepository>();
        mockStopRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(stops);

        var handler = new GetDelayHeatmapHandler(mockDelayRepo.Object, mockStopRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetDelayHeatmapQuery(from, to), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(30.0, result[0].AvgDelaySeconds);
    }
}
