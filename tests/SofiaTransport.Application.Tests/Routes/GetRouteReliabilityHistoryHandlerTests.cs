using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Routes;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Tests.Routes;

public class GetRouteReliabilityHistoryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsHistoryFilteredByDate()
    {
        // Arrange
        var scores = new List<ReliabilityScore>
        {
            new() { RouteId = "r-204", ScoreDate = new DateTime(2026, 4, 1), OnTimePct = 0.90, AvgDelaySeconds = 30, Score = 85, PeakScore = 80 },
            new() { RouteId = "r-204", ScoreDate = new DateTime(2026, 4, 15), OnTimePct = 0.85, AvgDelaySeconds = 45, Score = 80, PeakScore = 75 },
            new() { RouteId = "r-204", ScoreDate = new DateTime(2026, 4, 28), OnTimePct = 0.92, AvgDelaySeconds = 20, Score = 88, PeakScore = 85 }
        };

        var mockRepo = new Mock<IReliabilityScoreRepository>();
        mockRepo.Setup(r => r.GetByRouteAsync("r-204")).ReturnsAsync(scores);

        var handler = new GetRouteReliabilityHistoryHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetRouteReliabilityHistoryQuery("r-204", new DateTime(2026, 4, 10), new DateTime(2026, 4, 30)),
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.True(r.Date >= new DateTime(2026, 4, 10)));
        Assert.All(result, r => Assert.True(r.Date <= new DateTime(2026, 4, 30)));
    }

    [Fact]
    public async Task Handle_NoScores_ReturnsEmptyList()
    {
        // Arrange
        var mockRepo = new Mock<IReliabilityScoreRepository>();
        mockRepo.Setup(r => r.GetByRouteAsync("r-999")).ReturnsAsync(Array.Empty<ReliabilityScore>());

        var handler = new GetRouteReliabilityHistoryHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(new GetRouteReliabilityHistoryQuery("r-999"), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_DefaultDateRange_ReturnsLast30Days()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var scores = new List<ReliabilityScore>
        {
            new() { RouteId = "r-1", ScoreDate = today.AddDays(-5), OnTimePct = 0.95, AvgDelaySeconds = 10, Score = 90, PeakScore = 88 },
            new() { RouteId = "r-1", ScoreDate = today.AddDays(-35), OnTimePct = 0.80, AvgDelaySeconds = 60, Score = 70, PeakScore = 65 }
        };

        var mockRepo = new Mock<IReliabilityScoreRepository>();
        mockRepo.Setup(r => r.GetByRouteAsync("r-1")).ReturnsAsync(scores);

        var handler = new GetRouteReliabilityHistoryHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(new GetRouteReliabilityHistoryQuery("r-1"), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(today.AddDays(-5), result[0].Date);
    }
}
