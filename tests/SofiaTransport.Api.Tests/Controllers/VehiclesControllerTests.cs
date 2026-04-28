using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Api.Controllers;
using SofiaTransport.Application.Vehicles;

namespace SofiaTransport.Api.Tests.Controllers;

public class VehiclesControllerTests
{
    private readonly VehiclesController _controller;
    private readonly Mock<IMediator> _mockMediator;

    public VehiclesControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new VehiclesController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetLive_ReturnsVehiclesWithoutFilter()
    {
        // Arrange
        var vehicles = new List<VehicleDto>
        {
            new("v-1001", "r-204", "t-500", 42.6977, 23.3219, 90f, 30f, DateTime.UtcNow),
            new("v-1002", "r-604", "t-501", 42.6939, 23.3451, 180f, 25f, DateTime.UtcNow)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetLiveVehiclesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicles);

        // Act
        var result = await _controller.GetLive(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<VehicleDto>>(okResult.Value);
        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public async Task GetLive_PassesRouteIdFilter()
    {
        // Arrange
        GetLiveVehiclesQuery? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetLiveVehiclesQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<VehicleDto>>, CancellationToken>((q, _) => capturedQuery = (GetLiveVehiclesQuery)q)
            .ReturnsAsync(new List<VehicleDto>());

        // Act
        await _controller.GetLive("r-204");

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal("r-204", capturedQuery.RouteId);
    }
}
