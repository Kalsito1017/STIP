using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Shapes;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Application.Tests.Shapes;

public class GetAllRouteShapesHandlerTests
{
    [Fact]
    public async Task Handle_GroupsShapesByRouteId_ProducesGeoJson()
    {
        // Arrange
        var points = new List<Shape>
        {
            new() { RouteId = "r-1", Sequence = 1, Lat = 42.6897, Lon = 23.3342 },
            new() { RouteId = "r-1", Sequence = 2, Lat = 42.6900, Lon = 23.3400 },
            new() { RouteId = "r-204", Sequence = 1, Lat = 42.7000, Lon = 23.3200 },
        };

        var routes = new List<Route>
        {
            new() { RouteId = "r-1", ShortName = "1", Type = TransitType.Metro },
            new() { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus },
        };

        var mockShapeRepo = new Mock<IShapeRepository>();
        mockShapeRepo.Setup(r => r.GetAllGroupedByRouteAsync()).ReturnsAsync(points);

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(routes);

        var handler = new GetAllRouteShapesHandler(mockShapeRepo.Object, mockRouteRepo.Object);

        // Act
        var result = await handler.Handle(new GetAllRouteShapesQuery(), CancellationToken.None);

        // Assert
        Assert.Equal("FeatureCollection", result.Type);
        Assert.Equal(2, result.Features.Count);

        // First feature should be "r-1" (Metro)
        var feature1 = result.Features[0];
        Assert.Equal("r-1", feature1.Properties.RouteId);
        Assert.Equal("1", feature1.Properties.ShortName);
        Assert.Equal("Metro", feature1.Properties.RouteType);
        Assert.Equal("#059669", feature1.Properties.Color);
        Assert.Equal("Feature", feature1.Type);
        Assert.Equal("LineString", feature1.Geometry.Type);
        Assert.Equal(2, feature1.Geometry.Coordinates.Count);

        // Second feature should be "r-204" (Bus)
        var feature2 = result.Features[1];
        Assert.Equal("r-204", feature2.Properties.RouteId);
        Assert.Equal("204", feature2.Properties.ShortName);
        Assert.Equal("Bus", feature2.Properties.RouteType);
        Assert.Equal("#2563eb", feature2.Properties.Color);
        Assert.Single(feature2.Geometry.Coordinates);
    }

    [Fact]
    public async Task Handle_EmptyShapes_ReturnsEmptyFeatureCollection()
    {
        // Arrange
        var mockShapeRepo = new Mock<IShapeRepository>();
        mockShapeRepo.Setup(r => r.GetAllGroupedByRouteAsync()).ReturnsAsync(Array.Empty<Shape>());

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Route>());

        var handler = new GetAllRouteShapesHandler(mockShapeRepo.Object, mockRouteRepo.Object);

        // Act
        var result = await handler.Handle(new GetAllRouteShapesQuery(), CancellationToken.None);

        // Assert
        Assert.Equal("FeatureCollection", result.Type);
        Assert.Empty(result.Features);
    }

    [Fact]
    public async Task Handle_RouteWithoutName_UsesRouteIdAsFallback()
    {
        // Arrange
        var points = new List<Shape>
        {
            new() { RouteId = "r-unknown", Sequence = 1, Lat = 42.7, Lon = 23.3 },
        };

        var mockShapeRepo = new Mock<IShapeRepository>();
        mockShapeRepo.Setup(r => r.GetAllGroupedByRouteAsync()).ReturnsAsync(points);

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Route>());

        var handler = new GetAllRouteShapesHandler(mockShapeRepo.Object, mockRouteRepo.Object);

        // Act
        var result = await handler.Handle(new GetAllRouteShapesQuery(), CancellationToken.None);

        // Assert
        Assert.Single(result.Features);
        Assert.Equal("r-unknown", result.Features[0].Properties.RouteId);
        Assert.Equal("r-unknown", result.Features[0].Properties.ShortName);
        Assert.Equal("Bus", result.Features[0].Properties.RouteType);
    }
}
