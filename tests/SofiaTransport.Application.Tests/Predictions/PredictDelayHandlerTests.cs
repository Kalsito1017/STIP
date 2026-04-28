using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Predictions;

namespace SofiaTransport.Application.Tests.Predictions;

public class PredictDelayHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToMLService_ReturnsResponse()
    {
        // Arrange
        var expectedResponse = new PredictDelayResponse(
            PredictedDelaySeconds: 120.5,
            ConfidenceInterval: new List<double> { 90.0, 150.0 },
            ModelVersion: "v1.2.3"
        );

        var mockMLService = new Mock<IMLService>();
        mockMLService
            .Setup(m => m.PredictDelayAsync(
                "r-204", "s-001", 8, 1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var handler = new PredictDelayHandler(mockMLService.Object);

        // Act
        var result = await handler.Handle(
            new PredictDelayCommand("r-204", "s-001", 8, 1, 5),
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(120.5, result.PredictedDelaySeconds);
        Assert.Equal(2, result.ConfidenceInterval.Count);
        Assert.Equal(90.0, result.ConfidenceInterval[0]);
        Assert.Equal(150.0, result.ConfidenceInterval[1]);
        Assert.Equal("v1.2.3", result.ModelVersion);
    }

    [Fact]
    public async Task Handle_PassesAllParametersCorrectly()
    {
        // Arrange
        var mockMLService = new Mock<IMLService>();
        mockMLService
            .Setup(m => m.PredictDelayAsync(
                "r-1", "s-999", 23, 6, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PredictDelayResponse(0, [], "v0"));

        var handler = new PredictDelayHandler(mockMLService.Object);

        // Act
        await handler.Handle(
            new PredictDelayCommand("r-1", "s-999", 23, 6, 1),
            CancellationToken.None);

        // Assert
        mockMLService.Verify(
            m => m.PredictDelayAsync("r-1", "s-999", 23, 6, 1, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
