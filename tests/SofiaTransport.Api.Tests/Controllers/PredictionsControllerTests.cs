using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Api.Controllers;
using SofiaTransport.Application.Predictions;

namespace SofiaTransport.Api.Tests.Controllers;

public class PredictionsControllerTests
{
    private readonly PredictionsController _controller;
    private readonly Mock<IMediator> _mockMediator;

    public PredictionsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new PredictionsController(_mockMediator.Object);
    }

    [Fact]
    public async Task PredictDelay_ValidRequest_ReturnsOkWithResponse()
    {
        // Arrange
        var expectedResponse = new PredictDelayResponse(120, new List<double> { 60, 180 }, "v1.0");
        _mockMediator
            .Setup(m => m.Send(It.IsAny<PredictDelayCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var request = new SofiaTransport.Api.Controllers.PredictDelayRequest("r-204", "s-001", 8, 1, 5);

        // Act
        var result = await _controller.PredictDelay(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<PredictDelayResponse>(okResult.Value);
        Assert.Equal(120, actual.PredictedDelaySeconds);
        Assert.Equal("v1.0", actual.ModelVersion);
    }

    [Fact]
    public async Task PredictDelay_CorrectCommandIsSent_ForwardsParameters()
    {
        // Arrange
        var expectedResponse = new PredictDelayResponse(0, new List<double>(), "v1.0");
        PredictDelayCommand? capturedCommand = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<PredictDelayCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<PredictDelayResponse>, CancellationToken>((cmd, _) => capturedCommand = (PredictDelayCommand)cmd)
            .ReturnsAsync(expectedResponse);

        var request = new SofiaTransport.Api.Controllers.PredictDelayRequest("r-99", "s-42", 14, 3, 10);

        // Act
        await _controller.PredictDelay(request);

        // Assert
        Assert.NotNull(capturedCommand);
        Assert.Equal("r-99", capturedCommand.RouteId);
        Assert.Equal("s-42", capturedCommand.StopId);
        Assert.Equal(14, capturedCommand.Hour);
        Assert.Equal(3, capturedCommand.DayOfWeek);
        Assert.Equal(10, capturedCommand.StopSequence);
    }

    [Fact]
    public async Task PredictTravelTime_ValidRequest_ReturnsOkWithResponse()
    {
        // Arrange
        var expectedResponse = new TravelTimePredictionResponse(1200, new List<double> { 1080, 1320 }, "heuristic-v1");
        _mockMediator
            .Setup(m => m.Send(It.IsAny<PredictTravelTimeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var request = new SofiaTransport.Api.Controllers.PredictTravelTimeRequest("s-from", "s-to", "r-204", DateTime.UtcNow.AddHours(1));

        // Act
        var result = await _controller.PredictTravelTime(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<TravelTimePredictionResponse>(okResult.Value);
        Assert.Equal(1200, actual.PredictedTravelTimeSeconds);
        Assert.Equal("heuristic-v1", actual.ModelVersion);
    }

    [Fact]
    public async Task PredictTravelTime_CorrectCommandIsSent_ForwardsParameters()
    {
        // Arrange
        var expectedResponse = new TravelTimePredictionResponse(0, new List<double>(), "heuristic-v1");
        PredictTravelTimeCommand? capturedCommand = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<PredictTravelTimeCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<TravelTimePredictionResponse>, CancellationToken>((cmd, _) => capturedCommand = (PredictTravelTimeCommand)cmd)
            .ReturnsAsync(expectedResponse);

        var departure = DateTime.UtcNow.AddHours(1);
        var request = new SofiaTransport.Api.Controllers.PredictTravelTimeRequest("s-from", "s-to", "r-99", departure);

        // Act
        await _controller.PredictTravelTime(request);

        // Assert
        Assert.NotNull(capturedCommand);
        Assert.Equal("s-from", capturedCommand.FromStopId);
        Assert.Equal("s-to", capturedCommand.ToStopId);
        Assert.Equal("r-99", capturedCommand.RouteId);
        Assert.Equal(departure, capturedCommand.DepartureTime);
    }
}
