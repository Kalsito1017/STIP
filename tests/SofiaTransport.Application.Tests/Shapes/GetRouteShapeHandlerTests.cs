using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Shapes;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Application.Tests.Shapes;

public class GetRouteShapeHandlerTests
{
    [Fact]
    public async Task Handle_PointsExist_ReturnsRouteShapeCollection()
    {
        // Arrange
        var points = new List<Shape>
        {
            new() { RouteId = "r-1", Sequence = 1, Lat = 42.6897, Lon = 23.3342 },
            new() { RouteId = "r-1", Sequence = 2, Lat = 42.6900, Lon = 23.3400 },
        };

        var route = new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Metro };

        var mockShapeRepo = new Mock<IShapeRepository>();
        mockShapeRepo.Setup(r => r.GetByRouteIdAsync("r-1")).ReturnsAsync(points);

        var mockRouteRepo = new Mock<IRouteRepository>();
        mockRouteRepo.Setup(r => r.GetByIdAsync("r-1")).ReturnsAsync(route);

        var handler = new GetRouteShapeHandler(mockShapeRepo.Object, mockRouteRepo.Object);

        // Act
        var result = await handler.Handle(new GetRouteShapeQuery("r-1"), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FeatureCollection", result.Type);
        Assert.Single(result.Features);

        var feature = result.Features[0];
        Assert.Equal("Feature", feature.Type);
        Assert.Equal("LineString", feature.Geometry.Type);
        Assert.Equal(2, feature.Geometry.Coordinates.Count);
        Assert.Equal("r-1", feature.Properties.RouteId);
        Assert.Equal("1", feature.Properties.ShortName);
        Assert.Equal("Metro", feature.Properties.RouteType);
        Assert.Equal("#059669", feature.Properties.Color);
    }

    [Fact]
    public async Task Handle_NoPointsFound_ReturnsNull()
    {
        // Arrange
        var mockShapeRepo = new Mock<IShapeRepository>();
        mockShapeRepo.Setup(r => r.GetByRouteIdAsync("r-999")).ReturnsAsync(Array.Empty<Shape>());

        var mockRouteRepo = new Mock<IRouteRepository>();

        var handler = new GetRouteShapeHandler(mockShapeRepo.Object, mockRouteRepo.Object);

        // Act
        var result = await handler.Handle(new GetRouteShapeQuery("r-999"), CancellationToken.None);

        // Assert
        Assert.Null(result);
        mockRouteRepo.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(TransitType.Bus, "#2563eb")]
    [InlineData(TransitType.Tram, "#dc2626")]
    [InlineData(TransitType.Trolley, "#7c3aed")]
    [InlineData(TransitType.Metro, "#059669")]
    [InlineData(null, "#6b7280")]
    public void GetRouteColor_ReturnsCorrectColor(TransitType? type, string expectedColor)
    {
        // Act
        var result = GetRouteShapeHandler.GetRouteColor(type);

        // Assert
        Assert.Equal(expectedColor, result);
    }
}
