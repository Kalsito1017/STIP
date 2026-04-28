using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Vehicles;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Application.Tests.Vehicles;

public class GetLiveVehiclesHandlerTests
{
    [Fact]
    public async Task Handle_NoRouteFilter_ReturnsAllVehicles()
    {
        var vehicles = new List<Vehicle>
        {
            new() { VehicleId = "v1", RouteId = "r-1", Location = new Coordinates(42.69, 23.33), RecordedAt = DateTime.UtcNow },
            new() { VehicleId = "v2", RouteId = "r-204", Location = new Coordinates(42.68, 23.32), RecordedAt = DateTime.UtcNow },
        };

        var mockCache = new Mock<IVehicleCache>();
        mockCache.Setup(c => c.GetAllAsync()).ReturnsAsync(vehicles);

        var handler = new GetLiveVehiclesHandler(mockCache.Object);
        var result = await handler.Handle(new GetLiveVehiclesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_WithRouteFilter_ReturnsFilteredVehicles()
    {
        var vehicles = new List<Vehicle>
        {
            new() { VehicleId = "v1", RouteId = "r-1", Location = new Coordinates(42.69, 23.33), RecordedAt = DateTime.UtcNow },
            new() { VehicleId = "v2", RouteId = "r-204", Location = new Coordinates(42.68, 23.32), RecordedAt = DateTime.UtcNow },
            new() { VehicleId = "v3", RouteId = "r-1", Location = new Coordinates(42.70, 23.34), RecordedAt = DateTime.UtcNow },
        };

        var mockCache = new Mock<IVehicleCache>();
        mockCache.Setup(c => c.GetAllAsync()).ReturnsAsync(vehicles);

        var handler = new GetLiveVehiclesHandler(mockCache.Object);
        var result = await handler.Handle(new GetLiveVehiclesQuery("r-1"), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, v => Assert.Equal("r-1", v.RouteId));
    }

    [Fact]
    public async Task Handle_WithUnknownRouteFilter_ReturnsEmpty()
    {
        var vehicles = new List<Vehicle>
        {
            new() { VehicleId = "v1", RouteId = "r-1", Location = new Coordinates(42.69, 23.33), RecordedAt = DateTime.UtcNow },
        };

        var mockCache = new Mock<IVehicleCache>();
        mockCache.Setup(c => c.GetAllAsync()).ReturnsAsync(vehicles);

        var handler = new GetLiveVehiclesHandler(mockCache.Object);
        var result = await handler.Handle(new GetLiveVehiclesQuery("r-999"), CancellationToken.None);

        Assert.Empty(result);
    }
}
