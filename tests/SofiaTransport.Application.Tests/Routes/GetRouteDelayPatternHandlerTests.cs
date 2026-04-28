using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Routes;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Tests.Routes;

public class GetRouteDelayPatternHandlerTests
{
    [Fact]
    public async Task Handle_WithLogs_ReturnsHourlyDelayPattern()
    {
        // Arrange
        var targetDate = new DateTime(2026, 4, 27);
        var logs = new List<DelayLog>
        {
            new() { RouteId = "r-204", ScheduledArrival = targetDate.AddHours(8).AddMinutes(0), DelaySeconds = 60 },
            new() { RouteId = "r-204", ScheduledArrival = targetDate.AddHours(8).AddMinutes(30), DelaySeconds = 120 },
            new() { RouteId = "r-204", ScheduledArrival = targetDate.AddHours(9).AddMinutes(0), DelaySeconds = 30 },
        };

        var mockRepo = new Mock<IDelayLogRepository>();
        mockRepo
            .Setup(r => r.GetByRouteAsync("r-204", targetDate, targetDate.AddDays(1)))
            .ReturnsAsync(logs);

        var handler = new GetRouteDelayPatternHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetRouteDelayPatternQuery("r-204", targetDate), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(8, result[0].HourOfDay);
        Assert.Equal(90.0, result[0].AvgDelaySeconds); // (60+120)/2
        Assert.Equal(9, result[1].HourOfDay);
        Assert.Equal(30.0, result[1].AvgDelaySeconds);
    }

    [Fact]
    public async Task Handle_NoLogsForRoute_ReturnsEmptyList()
    {
        // Arrange
        var targetDate = new DateTime(2026, 4, 27);
        var mockRepo = new Mock<IDelayLogRepository>();
        mockRepo
            .Setup(r => r.GetByRouteAsync("r-999", targetDate, targetDate.AddDays(1)))
            .ReturnsAsync(Array.Empty<DelayLog>());

        var handler = new GetRouteDelayPatternHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetRouteDelayPatternQuery("r-999", targetDate), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_SingleHour_ReturnsOneEntry()
    {
        // Arrange
        var targetDate = new DateTime(2026, 4, 27);
        var logs = new List<DelayLog>
        {
            new() { RouteId = "r-204", ScheduledArrival = targetDate.AddHours(17).AddMinutes(5), DelaySeconds = 300 },
            new() { RouteId = "r-204", ScheduledArrival = targetDate.AddHours(17).AddMinutes(35), DelaySeconds = 200 },
            new() { RouteId = "r-204", ScheduledArrival = targetDate.AddHours(17).AddMinutes(55), DelaySeconds = 100 },
        };

        var mockRepo = new Mock<IDelayLogRepository>();
        mockRepo
            .Setup(r => r.GetByRouteAsync("r-204", targetDate, targetDate.AddDays(1)))
            .ReturnsAsync(logs);

        var handler = new GetRouteDelayPatternHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetRouteDelayPatternQuery("r-204", targetDate), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(17, result[0].HourOfDay);
        Assert.Equal(200.0, result[0].AvgDelaySeconds); // (300+200+100)/3
    }

    [Fact]
    public async Task Handle_MultipleHours_OrderedByHour()
    {
        // Arrange
        var targetDate = new DateTime(2026, 4, 27);
        var logs = new List<DelayLog>
        {
            new() { RouteId = "r-204", ScheduledArrival = targetDate.AddHours(14), DelaySeconds = 50 },
            new() { RouteId = "r-204", ScheduledArrival = targetDate.AddHours(8), DelaySeconds = 30 },
            new() { RouteId = "r-204", ScheduledArrival = targetDate.AddHours(20), DelaySeconds = 80 },
        };

        var mockRepo = new Mock<IDelayLogRepository>();
        mockRepo
            .Setup(r => r.GetByRouteAsync("r-204", targetDate, targetDate.AddDays(1)))
            .ReturnsAsync(logs);

        var handler = new GetRouteDelayPatternHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetRouteDelayPatternQuery("r-204", targetDate), CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(8, result[0].HourOfDay);
        Assert.Equal(14, result[1].HourOfDay);
        Assert.Equal(20, result[2].HourOfDay);
    }
}
