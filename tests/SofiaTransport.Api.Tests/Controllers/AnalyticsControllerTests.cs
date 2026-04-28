using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Api.Controllers;
using SofiaTransport.Application.Analytics;

namespace SofiaTransport.Api.Tests.Controllers;

public class AnalyticsControllerTests
{
    private readonly AnalyticsController _controller;
    private readonly Mock<IMediator> _mockMediator;

    public AnalyticsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new AnalyticsController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetOverview_ReturnsOkWithOverview()
    {
        // Arrange
        var overview = new SystemOverviewDto(50, 45.5, 120, 800);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSystemOverviewQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(overview);

        // Act
        var result = await _controller.GetOverview();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<SystemOverviewDto>(okResult.Value);
        Assert.Equal(50, actual.LiveVehicleCount);
        Assert.Equal(45.5, actual.AvgDelaySecondsLastHour);
        Assert.Equal(120, actual.TotalRoutes);
        Assert.Equal(800, actual.TotalStops);
    }

    [Fact]
    public async Task GetDelayHeatmap_ReturnsOkWithHeatmapData()
    {
        // Arrange
        var heatmapData = new List<HeatmapPointDto>
        {
            new(42.6977, 23.3219, 45.5, 10),
            new(42.6939, 23.3451, 30.0, 5)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetDelayHeatmapQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(heatmapData);

        // Act
        var result = await _controller.GetDelayHeatmap(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<HeatmapPointDto>>(okResult.Value);
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public async Task GetDelayHeatmap_PassesFromAndToQueryParams()
    {
        // Arrange
        var from = new DateTime(2026, 4, 1);
        var to = new DateTime(2026, 4, 7);
        GetDelayHeatmapQuery? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetDelayHeatmapQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<HeatmapPointDto>>, CancellationToken>((q, _) => capturedQuery = (GetDelayHeatmapQuery)q)
            .ReturnsAsync(new List<HeatmapPointDto>());

        // Act
        await _controller.GetDelayHeatmap(from, to);

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal(from, capturedQuery.From);
        Assert.Equal(to, capturedQuery.To);
    }

    [Fact]
    public async Task GetReliabilityRanking_ReturnsOkWithRankings()
    {
        // Arrange
        var rankings = new List<ReliabilityRankingDto>
        {
            new("r-1", "1", 95.0, 97.0, 30.0),
            new("r-204", "204", 85.0, 88.0, 60.0)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetReliabilityRankingQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rankings);

        // Act
        var result = await _controller.GetReliabilityRanking();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<ReliabilityRankingDto>>(okResult.Value);
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public async Task GetReliabilityRanking_DefaultParams_Top10BestTrue()
    {
        // Arrange
        GetReliabilityRankingQuery? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetReliabilityRankingQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<ReliabilityRankingDto>>, CancellationToken>((q, _) => capturedQuery = (GetReliabilityRankingQuery)q)
            .ReturnsAsync(new List<ReliabilityRankingDto>());

        // Act
        await _controller.GetReliabilityRanking();

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal(10, capturedQuery.Top);
        Assert.True(capturedQuery.Best);
    }

    [Fact]
    public async Task GetReliabilityRanking_CustomParams_Top5BestFalse()
    {
        // Arrange
        GetReliabilityRankingQuery? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetReliabilityRankingQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<ReliabilityRankingDto>>, CancellationToken>((q, _) => capturedQuery = (GetReliabilityRankingQuery)q)
            .ReturnsAsync(new List<ReliabilityRankingDto>());

        // Act
        await _controller.GetReliabilityRanking(5, false);

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal(5, capturedQuery.Top);
        Assert.False(capturedQuery.Best);
    }

    [Fact]
    public async Task GetPeakHours_ReturnsOkWithData()
    {
        // Arrange
        var peakHours = new List<PeakHourDto>
        {
            new(8, 120.0, 50),
            new(17, 90.0, 45)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPeakHoursQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(peakHours);

        // Act
        var result = await _controller.GetPeakHours(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<PeakHourDto>>(okResult.Value);
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public async Task GetPeakHours_WithoutDateParam_PassesNull()
    {
        // Arrange
        GetPeakHoursQuery? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPeakHoursQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<PeakHourDto>>, CancellationToken>((q, _) => capturedQuery = (GetPeakHoursQuery)q)
            .ReturnsAsync(new List<PeakHourDto>());

        // Act
        await _controller.GetPeakHours(null);

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Null(capturedQuery.Date);
    }
}
