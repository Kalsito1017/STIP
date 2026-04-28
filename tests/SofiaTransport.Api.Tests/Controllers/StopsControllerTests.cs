using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Api.Controllers;
using SofiaTransport.Application.Stops;

namespace SofiaTransport.Api.Tests.Controllers;

public class StopsControllerTests
{
    private readonly StopsController _controller;
    private readonly Mock<IMediator> _mockMediator;

    public StopsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new StopsController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        var stops = new List<StopDto>
        {
            new("s-001", "Central Station", 42.6977, 23.3219),
            new("s-002", "Vitosha Blvd", 42.6939, 23.3451)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStopsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stops);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<StopDto>>(okResult.Value);
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public async Task GetNearby_ReturnsOk()
    {
        // Arrange
        var stops = new List<StopDto>
        {
            new("s-001", "Central Station", 42.6977, 23.3219)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetNearbyStopsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stops);

        // Act
        var result = await _controller.GetNearby(42.6977, 23.3219, 1.0);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<StopDto>>(okResult.Value);
        Assert.Single(actual);
    }

    [Fact]
    public async Task GetNearby_PassesQueryParams()
    {
        // Arrange
        GetNearbyStopsQuery? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetNearbyStopsQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<StopDto>>, CancellationToken>((q, _) => capturedQuery = (GetNearbyStopsQuery)q)
            .ReturnsAsync(new List<StopDto>());

        // Act
        await _controller.GetNearby(42.7, 23.3, 2.5);

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal(42.7, capturedQuery.Lat);
        Assert.Equal(23.3, capturedQuery.Lon);
        Assert.Equal(2.5, capturedQuery.RadiusKm);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenStopExists()
    {
        // Arrange
        var stop = new StopDto("s-001", "Central Station", 42.6977, 23.3219);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStopByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stop);

        // Act
        var result = await _controller.GetById("s-001");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<StopDto>(okResult.Value);
        Assert.Equal("s-001", actual.StopId);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenStopIsNull()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStopByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StopDto?)null);

        // Act
        var result = await _controller.GetById("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetCongestion_ReturnsOk()
    {
        // Arrange
        var congestion = new List<StopCongestionDto>
        {
            new(8, 25),
            new(9, 15),
            new(17, 30)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStopCongestionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(congestion);

        // Act
        var result = await _controller.GetCongestion("s-001", null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<StopCongestionDto>>(okResult.Value);
        Assert.Equal(3, actual.Count);
    }

    [Fact]
    public async Task GetCongestion_PassesDateParam()
    {
        // Arrange
        var date = new DateTime(2026, 4, 27);
        GetStopCongestionQuery? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStopCongestionQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<StopCongestionDto>>, CancellationToken>((q, _) => capturedQuery = (GetStopCongestionQuery)q)
            .ReturnsAsync(new List<StopCongestionDto>());

        // Act
        await _controller.GetCongestion("s-001", date);

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal("s-001", capturedQuery.StopId);
        Assert.Equal(date, capturedQuery.Date);
    }

    [Fact]
    public async Task GetPredictedArrivals_ReturnsOk()
    {
        // Arrange
        var arrivals = new List<PredictedArrivalDto>
        {
            new("r-204", "204", "G.M. Dimitrov", 5, 60, "v1.0"),
            new("r-604", "604", "Mladost", 12, null, "v1.0")
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPredictedArrivalsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(arrivals);

        // Act
        var result = await _controller.GetPredictedArrivals("s-001");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<PredictedArrivalDto>>(okResult.Value);
        Assert.Equal(2, actual.Count);
    }
}
