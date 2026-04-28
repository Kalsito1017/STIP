using Xunit;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Domain.Tests.Entities;

public class StopTimeTests
{
    [Fact]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        var stopTime = new StopTime();
        Assert.Equal(string.Empty, stopTime.TripId);
        Assert.Equal(string.Empty, stopTime.StopId);
        Assert.Equal(0, stopTime.StopSequence);
        Assert.Equal(TimeSpan.Zero, stopTime.ArrivalTime);
        Assert.Null(stopTime.Trip);
        Assert.Null(stopTime.Stop);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var trip = new Trip { TripId = "t-500" };
        var stop = new Stop { StopId = "s-001", StopName = "Orlov Most" };

        var stopTime = new StopTime
        {
            TripId = "t-500",
            StopId = "s-001",
            StopSequence = 3,
            ArrivalTime = TimeSpan.FromHours(8.25), // 08:15:00
            Trip = trip,
            Stop = stop
        };

        Assert.Equal("t-500", stopTime.TripId);
        Assert.Equal("s-001", stopTime.StopId);
        Assert.Equal(3, stopTime.StopSequence);
        Assert.Equal(TimeSpan.FromHours(8.25), stopTime.ArrivalTime);
        Assert.Equal(trip, stopTime.Trip);
        Assert.Equal(stop, stopTime.Stop);
    }

    [Fact]
    public void ArrivalTime_CanBeLateNight()
    {
        var stopTime = new StopTime
        {
            TripId = "t-001",
            StopId = "s-001",
            ArrivalTime = TimeSpan.FromHours(23.5) // 23:30:00
        };

        Assert.Equal(TimeSpan.FromHours(23.5), stopTime.ArrivalTime);
    }

    [Fact]
    public void ArrivalTime_CanExceed24Hours()
    {
        // GTFS allows times past midnight like 25:30:00 (1:30 AM next day)
        var stopTime = new StopTime
        {
            TripId = "t-001",
            StopId = "s-001",
            ArrivalTime = TimeSpan.FromHours(25.5) // 25:30:00
        };

        Assert.Equal(TimeSpan.FromHours(25.5), stopTime.ArrivalTime);
    }

    [Fact]
    public void StopSequence_CanBeLargeNumber()
    {
        var stopTime = new StopTime
        {
            TripId = "t-001",
            StopId = "s-050",
            StopSequence = 50
        };

        Assert.Equal(50, stopTime.StopSequence);
    }

    [Fact]
    public void NavigationProperties_AreIndependent()
    {
        var trip = new Trip { TripId = "t-500" };
        var stop = new Stop { StopId = "s-001", StopName = "Orlov Most" };

        var stopTime = new StopTime
        {
            TripId = "t-500",
            StopId = "s-001",
            Trip = trip,
            Stop = stop
        };

        // Verify navigation property integrity
        Assert.Equal("t-500", stopTime.Trip.TripId);
        Assert.Equal("Orlov Most", stopTime.Stop.StopName);
    }
}
