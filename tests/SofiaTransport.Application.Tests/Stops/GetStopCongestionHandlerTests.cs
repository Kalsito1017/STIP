using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Stops;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Tests.Stops;

public class GetStopCongestionHandlerTests
{
    [Fact]
    public async Task Handle_WithLogs_ReturnsHourlyCongestion()
    {
        // Arrange
        var targetDate = new DateTime(2026, 4, 27);
        var logs = new List<DelayLog>
        {
            new() { StopId = "s-001", ScheduledArrival = targetDate.AddHours(8).AddMinutes(0), DelaySeconds = 60 },
            new() { StopId = "s-001", ScheduledArrival = targetDate.AddHours(8).AddMinutes(15), DelaySeconds = 30 },
            new() { StopId = "s-001", ScheduledArrival = targetDate.AddHours(8).AddMinutes(45), DelaySeconds = 120 },
            new() { StopId = "s-001", ScheduledArrival = targetDate.AddHours(9).AddMinutes(10), DelaySeconds = 0 },
            new() { StopId = "s-001", ScheduledArrival = targetDate.AddHours(9).AddMinutes(30), DelaySeconds = 15 },
        };

        var mockRepo = new Mock<IDelayLogRepository>();
        mockRepo
            .Setup(r => r.GetByStopAsync("s-001", targetDate, targetDate.AddDays(1)))
            .ReturnsAsync(logs);

        var handler = new GetStopCongestionHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetStopCongestionQuery("s-001", targetDate), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(8, result[0].HourOfDay);
        Assert.Equal(3, result[0].VehicleCount);
        Assert.Equal(9, result[1].HourOfDay);
        Assert.Equal(2, result[1].VehicleCount);
    }

    [Fact]
    public async Task Handle_NoLogsForStop_ReturnsEmptyList()
    {
        // Arrange
        var targetDate = new DateTime(2026, 4, 27);
        var mockRepo = new Mock<IDelayLogRepository>();
        mockRepo
            .Setup(r => r.GetByStopAsync("s-999", targetDate, targetDate.AddDays(1)))
            .ReturnsAsync(Array.Empty<DelayLog>());

        var handler = new GetStopCongestionHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetStopCongestionQuery("s-999", targetDate), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_MultipleHours_OrderedByHour()
    {
        // Arrange
        var targetDate = new DateTime(2026, 4, 27);
        var logs = new List<DelayLog>
        {
            new() { StopId = "s-001", ScheduledArrival = targetDate.AddHours(17), DelaySeconds = 10 },
            new() { StopId = "s-001", ScheduledArrival = targetDate.AddHours(7), DelaySeconds = 20 },
            new() { StopId = "s-001", ScheduledArrival = targetDate.AddHours(12), DelaySeconds = 30 },
        };

        var mockRepo = new Mock<IDelayLogRepository>();
        mockRepo
            .Setup(r => r.GetByStopAsync("s-001", targetDate, targetDate.AddDays(1)))
            .ReturnsAsync(logs);

        var handler = new GetStopCongestionHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetStopCongestionQuery("s-001", targetDate), CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(7, result[0].HourOfDay);
        Assert.Equal(12, result[1].HourOfDay);
        Assert.Equal(17, result[2].HourOfDay);
    }
}
