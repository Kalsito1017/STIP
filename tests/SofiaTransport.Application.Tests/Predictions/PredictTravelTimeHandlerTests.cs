using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Predictions;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Application.Tests.Predictions;

public class PredictTravelTimeHandlerTests
{
    [Fact]
    public async Task Handle_WithMatchingTrips_ReturnsHeuristicPrediction()
    {
        // Arrange
        var fromStopTimes = new List<StopTime>
        {
            new() { TripId = "t-1", StopId = "s-from", StopSequence = 1, ArrivalTime = TimeSpan.FromMinutes(10), Trip = new Trip { TripId = "t-1", RouteId = "r-204" } },
            new() { TripId = "t-2", StopId = "s-from", StopSequence = 1, ArrivalTime = TimeSpan.FromMinutes(20), Trip = new Trip { TripId = "t-2", RouteId = "r-204" } }
        };

        var toStopTimes = new List<StopTime>
        {
            new() { TripId = "t-1", StopId = "s-to", StopSequence = 3, ArrivalTime = TimeSpan.FromMinutes(25), Trip = new Trip { TripId = "t-1", RouteId = "r-204" } },
            new() { TripId = "t-2", StopId = "s-to", StopSequence = 3, ArrivalTime = TimeSpan.FromMinutes(40), Trip = new Trip { TripId = "t-2", RouteId = "r-204" } }
        };

        var delayLogs = new List<DelayLog>
        {
            new() { RouteId = "r-204", DelaySeconds = 60, RecordedAt = DateTime.UtcNow.AddDays(-1) },
            new() { RouteId = "r-204", DelaySeconds = 120, RecordedAt = DateTime.UtcNow.AddDays(-2) }
        };

        var mockStopTimeRepo = new Mock<IStopTimeRepository>();
        mockStopTimeRepo.Setup(r => r.GetByStopAndRouteAsync("s-from", "r-204")).ReturnsAsync(fromStopTimes);
        mockStopTimeRepo.Setup(r => r.GetByStopAndRouteAsync("s-to", "r-204")).ReturnsAsync(toStopTimes);

        var mockDelayRepo = new Mock<IDelayLogRepository>();
        mockDelayRepo.Setup(r => r.GetByRouteAsync("r-204", It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(delayLogs);

        var handler = new PredictTravelTimeHandler(mockStopTimeRepo.Object, mockDelayRepo.Object);

        // Act
        var result = await handler.Handle(
            new PredictTravelTimeCommand("s-from", "s-to", "r-204", DateTime.UtcNow.AddHours(1)),
            CancellationToken.None);

        // Assert
        Assert.Equal("heuristic-v1", result.ModelVersion);
        // Travel times: (25-10)=15min=900s, (40-20)=20min=1200s => avg=1050s
        // Avg delay: (60+120)/2=90s
        // Predicted = 1050 + 90 = 1140
        Assert.Equal(1140, result.PredictedTravelTimeSeconds);
        Assert.Equal(2, result.ConfidenceInterval.Count);
    }

    [Fact]
    public async Task Handle_NoMatchingTrips_ReturnsZeroPrediction()
    {
        // Arrange
        var mockStopTimeRepo = new Mock<IStopTimeRepository>();
        mockStopTimeRepo.Setup(r => r.GetByStopAndRouteAsync("s-from", "r-999")).ReturnsAsync(Array.Empty<StopTime>());
        mockStopTimeRepo.Setup(r => r.GetByStopAndRouteAsync("s-to", "r-999")).ReturnsAsync(Array.Empty<StopTime>());

        var mockDelayRepo = new Mock<IDelayLogRepository>();
        mockDelayRepo.Setup(r => r.GetByRouteAsync("r-999", It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(Array.Empty<DelayLog>());

        var handler = new PredictTravelTimeHandler(mockStopTimeRepo.Object, mockDelayRepo.Object);

        // Act
        var result = await handler.Handle(
            new PredictTravelTimeCommand("s-from", "s-to", "r-999", DateTime.UtcNow.AddHours(1)),
            CancellationToken.None);

        // Assert
        Assert.Equal(0, result.PredictedTravelTimeSeconds);
        Assert.Equal("heuristic-v1", result.ModelVersion);
    }
}
