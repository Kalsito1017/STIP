using Xunit;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Domain.Tests.Entities;

public class ShapeTests
{
    [Fact]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        var shape = new Shape();
        Assert.Equal(0, shape.Id);
        Assert.Equal(string.Empty, shape.RouteId);
        Assert.Equal(0, shape.Sequence);
        Assert.Equal(0, shape.Lat);
        Assert.Equal(0, shape.Lon);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var shape = new Shape
        {
            Id = 100,
            RouteId = "r-204",
            Sequence = 5,
            Lat = 42.6977,
            Lon = 23.3219
        };

        Assert.Equal(100, shape.Id);
        Assert.Equal("r-204", shape.RouteId);
        Assert.Equal(5, shape.Sequence);
        Assert.Equal(42.6977, shape.Lat);
        Assert.Equal(23.3219, shape.Lon);
    }

    [Fact]
    public void Route_NavigationProperty_IsSettable()
    {
        var route = new Route { RouteId = "r-204", ShortName = "204" };
        var shape = new Shape
        {
            RouteId = "r-204",
            Route = route
        };

        Assert.NotNull(shape.Route);
        Assert.Equal("r-204", shape.Route.RouteId);
        Assert.Equal("204", shape.Route.ShortName);
    }

    [Fact]
    public void Coordinates_SupportNegativeValues()
    {
        var shape = new Shape { Lat = -42.6977, Lon = -23.3219 };
        Assert.Equal(-42.6977, shape.Lat);
        Assert.Equal(-23.3219, shape.Lon);
    }
}
