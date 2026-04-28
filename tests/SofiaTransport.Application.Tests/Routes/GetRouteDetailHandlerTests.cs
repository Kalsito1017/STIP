using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Routes;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Application.Tests.Routes;

public class GetRouteDetailHandlerTests
{
    [Fact]
    public async Task Handle_RouteExistsWithScores_ReturnsDetailWithReliability()
    {
        // Arrange
        var route = new Route
        {
            RouteId = "r-204",
            ShortName = "204",
            LongName = "Mladost - Lyulin",
            Type = TransitType.Bus
        };

        var scores = new List<ReliabilityScore>
        {
            new()
            {
                RouteId = "r-204",
                ScoreDate = new DateTime(2026, 4, 20),
                OnTimePct = 0.85,
                AvgDelaySeconds = 90,
                Score = 77.5,
                PeakScore = 70.0,
                SampleCount = 150
            },
            new()
            {
                RouteId = "r-204",
                ScoreDate = new DateTime(2026, 4, 27),
                OnTimePct = 0.92,
                AvgDelaySeconds = 45,
                Score = 88.25,
                PeakScore = 82.0,
                SampleCount = 200
            }
        };

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetByIdAsync("r-204")).ReturnsAsync(route);

        var mockScoreRepo = new Mock<IReliabilityScoreRepository>();
        mockScoreRepo.Setup(s => s.GetByRouteAsync("r-204")).ReturnsAsync(scores);

        var handler = new GetRouteDetailHandler(mockRouteRepo.Object, mockScoreRepo.Object);

        // Act
        var result = await handler.Handle(new GetRouteDetailQuery("r-204"), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("r-204", result.RouteId);
        Assert.Equal("204", result.ShortName);
        Assert.Equal("Mladost - Lyulin", result.LongName);
        Assert.Equal(TransitType.Bus, result.Type);
        Assert.NotNull(result.LatestReliability);
        Assert.Equal(0.92, result.LatestReliability.OnTimePct);
        Assert.Equal(45, result.LatestReliability.AvgDelaySeconds);
        Assert.Equal(88.25, result.LatestReliability.Score);
    }

    [Fact]
    public async Task Handle_RouteExistsWithoutScores_ReturnsDetailWithNullReliability()
    {
        // Arrange
        var route = new Route
        {
            RouteId = "r-1",
            ShortName = "1",
            Type = TransitType.Metro
        };

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetByIdAsync("r-1")).ReturnsAsync(route);

        var mockScoreRepo = new Mock<IReliabilityScoreRepository>();
        mockScoreRepo.Setup(s => s.GetByRouteAsync("r-1")).ReturnsAsync(Array.Empty<ReliabilityScore>());

        var handler = new GetRouteDetailHandler(mockRouteRepo.Object, mockScoreRepo.Object);

        // Act
        var result = await handler.Handle(new GetRouteDetailQuery("r-1"), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("r-1", result.RouteId);
        Assert.Null(result.LatestReliability);
    }

    [Fact]
    public async Task Handle_RouteNotFound_ReturnsNull()
    {
        // Arrange
        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetByIdAsync("r-999")).ReturnsAsync((Route?)null);

        var mockScoreRepo = new Mock<IReliabilityScoreRepository>();

        var handler = new GetRouteDetailHandler(mockRouteRepo.Object, mockScoreRepo.Object);

        // Act
        var result = await handler.Handle(new GetRouteDetailQuery("r-999"), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_MultipleScores_PicksLatestByScoreDate()
    {
        // Arrange
        var route = new Route { RouteId = "r-t1", ShortName = "T1", Type = TransitType.Tram };

        var scores = new List<ReliabilityScore>
        {
            new() { RouteId = "r-t1", ScoreDate = new DateTime(2026, 1, 1), Score = 50.0 },
            new() { RouteId = "r-t1", ScoreDate = new DateTime(2026, 3, 15), Score = 70.0 },
            new() { RouteId = "r-t1", ScoreDate = new DateTime(2026, 2, 10), Score = 60.0 },
        };

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetByIdAsync("r-t1")).ReturnsAsync(route);

        var mockScoreRepo = new Mock<IReliabilityScoreRepository>();
        mockScoreRepo.Setup(s => s.GetByRouteAsync("r-t1")).ReturnsAsync(scores);

        var handler = new GetRouteDetailHandler(mockRouteRepo.Object, mockScoreRepo.Object);

        // Act
        var result = await handler.Handle(new GetRouteDetailQuery("r-t1"), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.LatestReliability);
        Assert.Equal(70.0, result.LatestReliability.Score);
    }
}
