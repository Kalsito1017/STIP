using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Analytics;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Application.Tests.Analytics;

public class GetReliabilityRankingHandlerTests
{
    [Fact]
    public async Task Handle_BestRanking_ReturnsTopBestRoutes()
    {
        // Arrange
        var ranking = new List<ReliabilityScore>
        {
            new() { RouteId = "r-1", Score = 95.0, OnTimePct = 0.98, AvgDelaySeconds = 12 },
            new() { RouteId = "r-204", Score = 88.0, OnTimePct = 0.92, AvgDelaySeconds = 45 },
        };

        var routes = new List<Route>
        {
            new() { RouteId = "r-1", ShortName = "1", Type = TransitType.Metro },
            new() { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus },
        };

        var mockScoreRepo = new Mock<IReliabilityScoreRepository>();
        mockScoreRepo.Setup(s => s.GetRankingAsync(5, true)).ReturnsAsync(ranking);

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(routes);

        var handler = new GetReliabilityRankingHandler(mockScoreRepo.Object, mockRouteRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetReliabilityRankingQuery(5, true), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("r-1", result[0].RouteId);
        Assert.Equal("1", result[0].ShortName);
        Assert.Equal(95.0, result[0].Score);
        Assert.Equal("r-204", result[1].RouteId);
        Assert.Equal("204", result[1].ShortName);
        Assert.Equal(88.0, result[1].Score);
    }

    [Fact]
    public async Task Handle_WorstRanking_ReturnsTopWorstRoutes()
    {
        // Arrange
        var ranking = new List<ReliabilityScore>
        {
            new() { RouteId = "r-99", Score = 25.0, OnTimePct = 0.50, AvgDelaySeconds = 300 },
        };

        var routes = new List<Route>
        {
            new() { RouteId = "r-99", ShortName = "99", Type = TransitType.Bus },
        };

        var mockScoreRepo = new Mock<IReliabilityScoreRepository>();
        mockScoreRepo.Setup(s => s.GetRankingAsync(10, false)).ReturnsAsync(ranking);

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(routes);

        var handler = new GetReliabilityRankingHandler(mockScoreRepo.Object, mockRouteRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetReliabilityRankingQuery(10, false), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("r-99", result[0].RouteId);
        Assert.Equal("99", result[0].ShortName);
        Assert.Equal(25.0, result[0].Score);
    }

    [Fact]
    public async Task Handle_EmptyRanking_ReturnsEmptyList()
    {
        // Arrange
        var mockScoreRepo = new Mock<IReliabilityScoreRepository>();
        mockScoreRepo.Setup(s => s.GetRankingAsync(10, true))
            .ReturnsAsync(Array.Empty<ReliabilityScore>());

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Route>());

        var handler = new GetReliabilityRankingHandler(mockScoreRepo.Object, mockRouteRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetReliabilityRankingQuery(10, true), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_RouteNotInRouteDict_UsesRouteIdAsShortName()
    {
        // Arrange
        var ranking = new List<ReliabilityScore>
        {
            new() { RouteId = "r-missing", Score = 70.0, OnTimePct = 0.80, AvgDelaySeconds = 120 },
        };

        var routes = new List<Route>
        {
            new() { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus },
        };

        var mockScoreRepo = new Mock<IReliabilityScoreRepository>();
        mockScoreRepo.Setup(s => s.GetRankingAsync(10, true)).ReturnsAsync(ranking);

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(routes);

        var handler = new GetReliabilityRankingHandler(mockScoreRepo.Object, mockRouteRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetReliabilityRankingQuery(10, true), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("r-missing", result[0].RouteId);
        Assert.Equal("r-missing", result[0].ShortName); // falls back to RouteId
    }
}
