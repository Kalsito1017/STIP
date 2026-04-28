using Xunit;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Domain.Tests.Entities;

public class RouteTests
{
    [Fact]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        var route = new Route();
        Assert.Equal(string.Empty, route.RouteId);
        Assert.Equal(string.Empty, route.ShortName);
        Assert.Null(route.LongName);
        Assert.Equal(default(TransitType), route.Type);
        Assert.NotNull(route.Trips);
        Assert.Empty(route.Trips);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var route = new Route
        {
            RouteId = "r-204",
            ShortName = "204",
            LongName = "Mladost - Lyulin",
            Type = TransitType.Bus
        };

        Assert.Equal("r-204", route.RouteId);
        Assert.Equal("204", route.ShortName);
        Assert.Equal("Mladost - Lyulin", route.LongName);
        Assert.Equal(TransitType.Bus, route.Type);
    }

    [Fact]
    public void Type_SupportsAllTransitTypes()
    {
        var metroRoute = new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Metro };
        var busRoute = new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus };
        var tramRoute = new Route { RouteId = "r-t1", ShortName = "T1", Type = TransitType.Tram };
        var trolleyRoute = new Route { RouteId = "r-tb", ShortName = "TB", Type = TransitType.Trolley };

        Assert.Equal(TransitType.Metro, metroRoute.Type);
        Assert.Equal(TransitType.Bus, busRoute.Type);
        Assert.Equal(TransitType.Tram, tramRoute.Type);
        Assert.Equal(TransitType.Trolley, trolleyRoute.Type);
    }

    [Fact]
    public void Trips_Collection_IsMutable()
    {
        var route = new Route();
        var trip = new Trip { TripId = "t-001", RouteId = "r-1" };

        route.Trips.Add(trip);

        Assert.Single(route.Trips);
        Assert.Equal("t-001", route.Trips.First().TripId);
    }

    [Fact]
    public void LongName_CanBeNull()
    {
        var route = new Route { RouteId = "r-1", ShortName = "1", LongName = null };
        Assert.Null(route.LongName);
    }

    [Fact]
    public void LongName_CanBeEmptyString()
    {
        var route = new Route { RouteId = "r-1", ShortName = "1", LongName = "" };
        Assert.Equal("", route.LongName);
    }
}
