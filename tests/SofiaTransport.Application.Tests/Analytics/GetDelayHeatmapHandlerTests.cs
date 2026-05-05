using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Analytics;

namespace SofiaTransport.Application.Tests.Analytics;

public class GetDelayHeatmapHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToRepository_ReturnsAggregatedHeatmapPoints()
    {
        // Arrange
        var from = new DateTime(2026, 4, 20);
        var to = new DateTime(2026, 4, 27);

        var expected = new List<HeatmapPointDto>
        {
            new(42.6897, 23.3342, 90.0, 2),
            new(42.6871, 23.3186, 30.0, 1),
        };

        var mockDelayRepo = new Mock<IDelayLogRepository>();
        mockDelayRepo.Setup(r => r.GetHeatmapAggregatedAsync(from, to)).ReturnsAsync(expected);

        var handler = new GetDelayHeatmapHandler(mockDelayRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetDelayHeatmapQuery(from, to), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        mockDelayRepo.Verify(r => r.GetHeatmapAggregatedAsync(from, to), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var from = new DateTime(2026, 4, 20);
        var to = new DateTime(2026, 4, 27);

        var mockDelayRepo = new Mock<IDelayLogRepository>();
        mockDelayRepo.Setup(r => r.GetHeatmapAggregatedAsync(from, to))
            .ReturnsAsync(Array.Empty<HeatmapPointDto>());

        var handler = new GetDelayHeatmapHandler(mockDelayRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetDelayHeatmapQuery(from, to), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_DefaultDates_UsesLast7Days()
    {
        // Arrange
        var mockDelayRepo = new Mock<IDelayLogRepository>();
        mockDelayRepo.Setup(r => r.GetHeatmapAggregatedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Array.Empty<HeatmapPointDto>());

        var handler = new GetDelayHeatmapHandler(mockDelayRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetDelayHeatmapQuery(), CancellationToken.None);

        // Assert
        Assert.Empty(result);
        mockDelayRepo.Verify(r => r.GetHeatmapAggregatedAsync(
            It.Is<DateTime>(d => d >= DateTime.UtcNow.AddMinutes(-1)),
            It.Is<DateTime>(d => d <= DateTime.UtcNow.AddMinutes(1))),
            Times.Once);
    }
}
