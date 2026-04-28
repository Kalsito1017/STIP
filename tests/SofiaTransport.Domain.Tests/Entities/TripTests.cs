using Xunit;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Domain.Tests.Entities;

public class TripTests
{
    [Fact]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        var trip = new Trip();
        Assert.Equal(string.Empty, trip.TripId);
        Assert.Equal(string.Empty, trip.RouteId);
        Assert.Equal(string.Empty, trip.ServiceId);
        Assert.Equal(0, trip.DirectionId);
        Assert.Null(trip.Route);
        Assert.NotNull(trip.StopTimes);
        Assert.Empty(trip.StopTimes);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var route = new Route { RouteId = "r-204", ShortName = "204" };

        var trip = new Trip
        {
            TripId = "t-500",
            RouteId = "r-204",
            ServiceId = "svc-weekday",
            DirectionId = 1,
            Route = route
        };

        Assert.Equal("t-500", trip.TripId);
        Assert.Equal("r-204", trip.RouteId);
        Assert.Equal("svc-weekday", trip.ServiceId);
        Assert.Equal(1, trip.DirectionId);
        Assert.Equal(route, trip.Route);
        Assert.Equal("204", trip.Route.ShortName);
    }

    [Fact]
    public void StopTimes_Collection_IsMutable()
    {
        var trip = new Trip { TripId = "t-500" };
        var stopTime = new StopTime { TripId = "t-500", StopId = "s-001", StopSequence = 1 };

        trip.StopTimes.Add(stopTime);

        Assert.Single(trip.StopTimes);
        Assert.Equal("s-001", trip.StopTimes.First().StopId);
        Assert.Equal(1, trip.StopTimes.First().StopSequence);
    }

    [Fact]
    public void DirectionId_CanBeZero()
    {
        var trip = new Trip { TripId = "t-001", DirectionId = 0 };
        Assert.Equal(0, trip.DirectionId);
    }

    [Fact]
    public void DirectionId_CanBeOne()
    {
        var trip = new Trip { TripId = "t-001", DirectionId = 1 };
        Assert.Equal(1, trip.DirectionId);
    }
}
