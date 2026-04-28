using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Stops;
using SofiaTransport.Application.Predictions;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Application.Tests.Stops;

public class GetPredictedArrivalsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPredictedArrivals()
    {
        // Arrange
        var stopTimes = new List<StopTime>
        {
            new()
            {
                TripId = "t-1",
                StopId = "s-001",
                StopSequence = 1,
                ArrivalTime = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)),
                Trip = new Trip
                {
                    TripId = "t-1",
                    RouteId = "r-204",
                    Route = new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus }
                }
            }
        };

        var mockStopTimeRepo = new Mock<IStopTimeRepository>();
        mockStopTimeRepo.Setup(r => r.GetUpcomingByStopAsync("s-001", It.IsAny<TimeSpan>(), 5)).ReturnsAsync(stopTimes);

        var mockMLService = new Mock<IMLService>();
        mockMLService.Setup(m => m.PredictDelayAsync("r-204", "s-001", It.IsAny<int>(), It.IsAny<int>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PredictDelayResponse(60, new List<double> { 30, 90 }, "v1.0"));

        var handler = new GetPredictedArrivalsHandler(mockStopTimeRepo.Object, mockMLService.Object);

        // Act
        var result = await handler.Handle(new GetPredictedArrivalsQuery("s-001"), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("r-204", result[0].RouteId);
        Assert.Equal("204", result[0].RouteShortName);
        Assert.Equal(60, result[0].PredictedDelaySeconds);
    }

    [Fact]
    public async Task Handle_NoUpcomingStopTimes_ReturnsEmptyList()
    {
        // Arrange
        var mockStopTimeRepo = new Mock<IStopTimeRepository>();
        mockStopTimeRepo.Setup(r => r.GetUpcomingByStopAsync("s-001", It.IsAny<TimeSpan>(), 5)).ReturnsAsync(Array.Empty<StopTime>());

        var mockMLService = new Mock<IMLService>();

        var handler = new GetPredictedArrivalsHandler(mockStopTimeRepo.Object, mockMLService.Object);

        // Act
        var result = await handler.Handle(new GetPredictedArrivalsQuery("s-001"), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
