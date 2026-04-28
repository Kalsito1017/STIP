using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Api.Controllers;
using SofiaTransport.Application.Routes;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Api.Tests.Controllers;

public class RoutesControllerTests
{
    private readonly RoutesController _controller;
    private readonly Mock<IMediator> _mockMediator;

    public RoutesControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new RoutesController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        var routes = new List<RouteDto>
        {
            new("r-1", "1", "Metro Line 1", TransitType.Metro),
            new("r-204", "204", null, TransitType.Bus)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRoutesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(routes);

        // Act
        var result = await _controller.GetAll(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<RouteDto>>(okResult.Value);
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public async Task GetAll_WithTypeFilter_PassesType()
    {
        // Arrange
        GetRoutesQuery? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRoutesQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<RouteDto>>, CancellationToken>((q, _) => capturedQuery = (GetRoutesQuery)q)
            .ReturnsAsync(new List<RouteDto>());

        // Act
        await _controller.GetAll(TransitType.Bus);

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal(TransitType.Bus, capturedQuery.Type);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenRouteExists()
    {
        // Arrange
        var route = new RouteDetailDto("r-1", "1", "Metro Line 1", TransitType.Metro,
            new ReliabilityDto(97.0, 30.0, 95.0, 85.0, 100));

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRouteDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(route);

        // Act
        var result = await _controller.GetById("r-1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<RouteDetailDto>(okResult.Value);
        Assert.Equal("r-1", actual.RouteId);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenRouteIsNull()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRouteDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RouteDetailDto?)null);

        // Act
        var result = await _controller.GetById("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetReliability_ReturnsOk_WhenRouteExists()
    {
        // Arrange
        var reliability = new ReliabilityDto(97.0, 30.0, 95.0, 85.0, 100);
        var route = new RouteDetailDto("r-1", "1", "Metro Line 1", TransitType.Metro, reliability);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRouteDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(route);

        // Act
        var result = await _controller.GetReliability("r-1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<ReliabilityDto>(okResult.Value);
        Assert.Equal(97.0, actual.OnTimePct);
    }

    [Fact]
    public async Task GetReliability_ReturnsNotFound_WhenRouteIsNull()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRouteDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RouteDetailDto?)null);

        // Act
        var result = await _controller.GetReliability("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetReliabilityHistory_ReturnsOk()
    {
        // Arrange
        var history = new List<ReliabilityHistoryDto>
        {
            new(new DateTime(2026, 4, 27), 0.92, 45.0, 88.0, 82.0),
            new(new DateTime(2026, 4, 26), 0.90, 50.0, 85.0, 80.0)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRouteReliabilityHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetReliabilityHistory("r-1", null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<ReliabilityHistoryDto>>(okResult.Value);
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public async Task GetReliabilityHistory_PassesDateParams()
    {
        // Arrange
        var from = new DateTime(2026, 4, 1);
        var to = new DateTime(2026, 4, 30);
        GetRouteReliabilityHistoryQuery? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRouteReliabilityHistoryQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<ReliabilityHistoryDto>>, CancellationToken>((q, _) => capturedQuery = (GetRouteReliabilityHistoryQuery)q)
            .ReturnsAsync(new List<ReliabilityHistoryDto>());

        // Act
        await _controller.GetReliabilityHistory("r-1", from, to);

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal("r-1", capturedQuery.RouteId);
        Assert.Equal(from, capturedQuery.From);
        Assert.Equal(to, capturedQuery.To);
    }

    [Fact]
    public async Task GetDelayPattern_ReturnsOk()
    {
        // Arrange
        var patterns = new List<DelayPatternDto>
        {
            new(8, 60.0),
            new(9, 45.0),
            new(17, 90.0)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRouteDelayPatternQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(patterns);

        // Act
        var result = await _controller.GetDelayPattern("r-204", null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<DelayPatternDto>>(okResult.Value);
        Assert.Equal(3, actual.Count);
    }

    [Fact]
    public async Task GetDelayPattern_PassesDateParam()
    {
        // Arrange
        var date = new DateTime(2026, 4, 27);
        GetRouteDelayPatternQuery? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRouteDelayPatternQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<DelayPatternDto>>, CancellationToken>((q, _) => capturedQuery = (GetRouteDelayPatternQuery)q)
            .ReturnsAsync(new List<DelayPatternDto>());

        // Act
        await _controller.GetDelayPattern("r-204", date);

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal("r-204", capturedQuery.RouteId);
        Assert.Equal(date, capturedQuery.Date);
    }
}
