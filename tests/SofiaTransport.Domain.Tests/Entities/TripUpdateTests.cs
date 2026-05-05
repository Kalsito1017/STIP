using Xunit;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Domain.Tests.Entities;

public class TripUpdateTests
{
    [Fact]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        var tu = new TripUpdate();
        Assert.Equal(string.Empty, tu.TripId);
        Assert.Null(tu.RouteId);
        Assert.Null(tu.StartTime);
        Assert.Null(tu.StartDate);
        Assert.Equal(0, tu.ScheduleRelationship);
        Assert.Null(tu.VehicleId);
        Assert.NotNull(tu.StopTimeUpdates);
        Assert.Empty(tu.StopTimeUpdates);
        Assert.Equal(default, tu.RecordedAt);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var tu = new TripUpdate
        {
            TripId = "t-001",
            RouteId = "r-204",
            StartTime = "08:00:00",
            StartDate = "20260501",
            ScheduleRelationship = 0,
            VehicleId = "v-001",
            RecordedAt = new DateTime(2026, 5, 1, 8, 0, 0)
        };

        Assert.Equal("t-001", tu.TripId);
        Assert.Equal("r-204", tu.RouteId);
        Assert.Equal("08:00:00", tu.StartTime);
        Assert.Equal("20260501", tu.StartDate);
        Assert.Equal(0, tu.ScheduleRelationship);
        Assert.Equal("v-001", tu.VehicleId);
        Assert.Equal(new DateTime(2026, 5, 1, 8, 0, 0), tu.RecordedAt);
    }

    [Fact]
    public void StopTimeUpdates_Collection_IsMutable()
    {
        var tu = new TripUpdate();
        var stu = new StopTimeUpdate
        {
            StopSequence = 1,
            StopId = "s-001",
            ArrivalDelay = 120
        };
        tu.StopTimeUpdates.Add(stu);

        Assert.Single(tu.StopTimeUpdates);
        Assert.Equal(1, tu.StopTimeUpdates[0].StopSequence);
        Assert.Equal("s-001", tu.StopTimeUpdates[0].StopId);
        Assert.Equal(120, tu.StopTimeUpdates[0].ArrivalDelay);
    }

    [Fact]
    public void RouteId_CanBeNull()
    {
        var tu = new TripUpdate { RouteId = null };
        Assert.Null(tu.RouteId);
    }

    [Fact]
    public void VehicleId_CanBeNull()
    {
        var tu = new TripUpdate { VehicleId = null };
        Assert.Null(tu.VehicleId);
    }
}

public class StopTimeUpdateTests
{
    [Fact]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        var stu = new StopTimeUpdate();
        Assert.Null(stu.StopSequence);
        Assert.Null(stu.StopId);
        Assert.Null(stu.ArrivalDelay);
        Assert.Null(stu.ArrivalTime);
        Assert.Null(stu.DepartureDelay);
        Assert.Null(stu.DepartureTime);
        Assert.Equal(0, stu.ScheduleRelationship);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var stu = new StopTimeUpdate
        {
            StopSequence = 3,
            StopId = "s-001",
            ArrivalDelay = 60,
            ArrivalTime = 28800,
            DepartureDelay = 90,
            DepartureTime = 28830,
            ScheduleRelationship = 0
        };

        Assert.Equal(3, stu.StopSequence);
        Assert.Equal("s-001", stu.StopId);
        Assert.Equal(60, stu.ArrivalDelay);
        Assert.Equal(28800, stu.ArrivalTime);
        Assert.Equal(90, stu.DepartureDelay);
        Assert.Equal(28830, stu.DepartureTime);
        Assert.Equal(0, stu.ScheduleRelationship);
    }

    [Fact]
    public void NullableFields_AcceptNull()
    {
        var stu = new StopTimeUpdate
        {
            StopSequence = null,
            StopId = null,
            ArrivalDelay = null,
            ArrivalTime = null,
            DepartureDelay = null,
            DepartureTime = null
        };

        Assert.Null(stu.StopSequence);
        Assert.Null(stu.StopId);
        Assert.Null(stu.ArrivalDelay);
        Assert.Null(stu.ArrivalTime);
        Assert.Null(stu.DepartureDelay);
        Assert.Null(stu.DepartureTime);
    }
}
