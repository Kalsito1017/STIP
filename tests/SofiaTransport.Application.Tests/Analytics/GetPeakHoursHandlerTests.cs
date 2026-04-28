using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Analytics;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Tests.Analytics;

public class GetPeakHoursHandlerTests
{
    [Fact]
    public async Task Handle_WithLogs_ReturnsPeakHourData()
    {
        // Arrange
        var targetDate = new DateTime(2026, 4, 27);
        var logs = new List<DelayLog>
        {
            new() { ScheduledArrival = targetDate.AddHours(8).AddMinutes(0), DelaySeconds = 60 },
            new() { ScheduledArrival = targetDate.AddHours(8).AddMinutes(30), DelaySeconds = 120 },
            new() { ScheduledArrival = targetDate.AddHours(9).AddMinutes(0), DelaySeconds = 30 },
            new() { ScheduledArrival = targetDate.AddHours(9).AddMinutes(30), DelaySeconds = 50 },
        };

        var mockRepo = new Mock<IDelayLogRepository>();
        mockRepo
            .Setup(r => r.GetByDateAsync(targetDate))
            .ReturnsAsync(logs);

        var handler = new GetPeakHoursHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetPeakHoursQuery(targetDate), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(8, result[0].HourOfDay);
        Assert.Equal(90.0, result[0].AvgDelaySeconds); // (60+120)/2
        Assert.Equal(2, result[0].VehicleCount);
        Assert.Equal(9, result[1].HourOfDay);
        Assert.Equal(40.0, result[1].AvgDelaySeconds); // (30+50)/2
        Assert.Equal(2, result[1].VehicleCount);
    }

    [Fact]
    public async Task Handle_NoLogs_ReturnsEmptyList()
    {
        // Arrange
        var targetDate = new DateTime(2026, 4, 27);
        var mockRepo = new Mock<IDelayLogRepository>();
        mockRepo
            .Setup(r => r.GetByDateAsync(targetDate))
            .ReturnsAsync(Array.Empty<DelayLog>());

        var handler = new GetPeakHoursHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetPeakHoursQuery(targetDate), CancellationToken.None);

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
            new() { ScheduledArrival = targetDate.AddHours(18), DelaySeconds = 200 },
            new() { ScheduledArrival = targetDate.AddHours(6), DelaySeconds = 10 },
            new() { ScheduledArrival = targetDate.AddHours(12), DelaySeconds = 50 },
        };

        var mockRepo = new Mock<IDelayLogRepository>();
        mockRepo
            .Setup(r => r.GetByDateAsync(targetDate))
            .ReturnsAsync(logs);

        var handler = new GetPeakHoursHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetPeakHoursQuery(targetDate), CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(6, result[0].HourOfDay);
        Assert.Equal(12, result[1].HourOfDay);
        Assert.Equal(18, result[2].HourOfDay);
    }
}
