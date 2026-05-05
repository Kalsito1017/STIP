using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Api.Controllers;
using SofiaTransport.Application.TripUpdates;

namespace SofiaTransport.Api.Tests.Controllers;

public class TripUpdatesControllerTests
{
    private readonly TripUpdatesController _controller;
    private readonly Mock<IMediator> _mockMediator;

    public TripUpdatesControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new TripUpdatesController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetLive_ReturnsOkWithUpdates()
    {
        // Arrange
        var updates = new List<TripUpdateDto>
        {
            new(
                "t-500", "r-204", "08:15:00", "20260505", 0, "v-1001",
                new List<StopTimeUpdateDto>
                {
                    new(1, "s-100", 120, 1714900500, 120, 1714900560, 0)
                },
                DateTime.UtcNow
            )
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetLiveTripUpdatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updates);

        // Act
        var result = await _controller.GetLive(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<TripUpdateDto>>(okResult.Value);
        Assert.Single(actual);
        Assert.Equal("t-500", actual[0].TripId);
    }

    [Fact]
    public async Task GetLive_WithRouteFilter_PassesRouteId()
    {
        // Arrange
        GetLiveTripUpdatesQuery? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetLiveTripUpdatesQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<TripUpdateDto>>, CancellationToken>((q, _) => capturedQuery = (GetLiveTripUpdatesQuery)q)
            .ReturnsAsync(new List<TripUpdateDto>());

        // Act
        await _controller.GetLive("r-204");

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal("r-204", capturedQuery.RouteId);
    }

    [Fact]
    public async Task GetLive_EmptyResult_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetLiveTripUpdatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TripUpdateDto>());

        // Act
        var result = await _controller.GetLive(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<TripUpdateDto>>(okResult.Value);
        Assert.Empty(actual);
    }
}
