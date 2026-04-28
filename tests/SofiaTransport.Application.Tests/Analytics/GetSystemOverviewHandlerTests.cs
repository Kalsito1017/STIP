using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Analytics;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Application.Tests.Analytics;

public class GetSystemOverviewHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOverview()
    {
        // Arrange
        var vehicles = new List<Vehicle>
        {
            new() { VehicleId = "v-1", RouteId = "r-1", Location = new Domain.ValueObjects.Coordinates(42.7, 23.3) },
            new() { VehicleId = "v-2", RouteId = "r-2", Location = new Domain.ValueObjects.Coordinates(42.71, 23.31) }
        };

        var routes = new List<Route>
        {
            new() { RouteId = "r-1", ShortName = "1", Type = TransitType.Metro },
            new() { RouteId = "r-2", ShortName = "2", Type = TransitType.Tram }
        };

        var stops = new List<Stop>
        {
            new() { StopId = "s-1", StopName = "Stop 1", Location = new Domain.ValueObjects.Coordinates(42.7, 23.3) },
            new() { StopId = "s-2", StopName = "Stop 2", Location = new Domain.ValueObjects.Coordinates(42.71, 23.31) },
            new() { StopId = "s-3", StopName = "Stop 3", Location = new Domain.ValueObjects.Coordinates(42.72, 23.32) }
        };

        var logs = new List<DelayLog>
        {
            new() { DelaySeconds = 60, RecordedAt = DateTime.UtcNow.AddMinutes(-30) },
            new() { DelaySeconds = 120, RecordedAt = DateTime.UtcNow.AddMinutes(-20) }
        };

        var mockVehicleCache = new Mock<IVehicleCache>();
        mockVehicleCache.Setup(c => c.GetAllAsync()).ReturnsAsync(vehicles);

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(routes);

        var mockStopRepo = new Mock<IStopRepository>();
        mockStopRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(stops);

        var mockDelayRepo = new Mock<IDelayLogRepository>();
        mockDelayRepo.Setup(r => r.GetForHeatmapAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(logs);

        var handler = new GetSystemOverviewHandler(
            mockVehicleCache.Object, mockDelayRepo.Object, mockRouteRepo.Object, mockStopRepo.Object);

        // Act
        var result = await handler.Handle(new GetSystemOverviewQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.LiveVehicleCount);
        Assert.Equal(2, result.TotalRoutes);
        Assert.Equal(3, result.TotalStops);
        Assert.Equal(90.0, result.AvgDelaySecondsLastHour); // (60+120)/2
    }

    [Fact]
    public async Task Handle_NoVehicles_ReturnsZeroCount()
    {
        // Arrange
        var mockVehicleCache = new Mock<IVehicleCache>();
        mockVehicleCache.Setup(c => c.GetAllAsync()).ReturnsAsync(Array.Empty<Vehicle>());

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Route>());

        var mockStopRepo = new Mock<IStopRepository>();
        mockStopRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Stop>());

        var mockDelayRepo = new Mock<IDelayLogRepository>();
        mockDelayRepo.Setup(r => r.GetForHeatmapAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(Array.Empty<DelayLog>());

        var handler = new GetSystemOverviewHandler(
            mockVehicleCache.Object, mockDelayRepo.Object, mockRouteRepo.Object, mockStopRepo.Object);

        // Act
        var result = await handler.Handle(new GetSystemOverviewQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(0, result.LiveVehicleCount);
        Assert.Equal(0, result.TotalRoutes);
        Assert.Equal(0, result.TotalStops);
        Assert.Equal(0, result.AvgDelaySecondsLastHour);
    }
}
