using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Api.Controllers;
using SofiaTransport.Application.Alerts;

namespace SofiaTransport.Api.Tests.Controllers;

public class AlertsControllerTests
{
    private readonly AlertsController _controller;
    private readonly Mock<IMediator> _mockMediator;

    public AlertsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new AlertsController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetActive_ReturnsOkWithAlerts()
    {
        // Arrange
        var alerts = new List<ServiceAlertDto>
        {
            new(
                "a-1", "Detour on Line 204", "Buses bypass stop 1234 due to construction.",
                "https://sumc.bg/alerts/1", 1, 8, 3,
                new List<ActivePeriodDto> { new(1714867200, 1714953600) },
                new List<InformedEntityDto>
                {
                    new(null, "r-204", null, null, "s-1234")
                },
                DateTime.UtcNow
            )
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetActiveAlertsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerts);

        // Act
        var result = await _controller.GetActive(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<ServiceAlertDto>>(okResult.Value);
        Assert.Single(actual);
        Assert.Equal("a-1", actual[0].AlertId);
    }

    [Fact]
    public async Task GetActive_WithRouteFilter_PassesRouteId()
    {
        // Arrange
        GetActiveAlertsQuery? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetActiveAlertsQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<ServiceAlertDto>>, CancellationToken>((q, _) => capturedQuery = (GetActiveAlertsQuery)q)
            .ReturnsAsync(new List<ServiceAlertDto>());

        // Act
        await _controller.GetActive("r-204");

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal("r-204", capturedQuery.RouteId);
    }

    [Fact]
    public async Task GetActive_EmptyResult_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetActiveAlertsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceAlertDto>());

        // Act
        var result = await _controller.GetActive(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<ServiceAlertDto>>(okResult.Value);
        Assert.Empty(actual);
    }
}
