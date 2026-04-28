using Xunit;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Domain.Tests.Entities;

public class DelayLogTests
{
    [Fact]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        var log = new DelayLog();
        Assert.Equal(0L, log.Id);
        Assert.Null(log.VehicleId);
        Assert.Null(log.StopId);
        Assert.Null(log.TripId);
        Assert.Null(log.RouteId);
        Assert.Equal(default(DateTime), log.ScheduledArrival);
        Assert.Equal(default(DateTime), log.ActualArrival);
        Assert.Equal(0, log.DelaySeconds);
        Assert.Equal(default(DateTime), log.RecordedAt);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var scheduled = new DateTime(2026, 4, 27, 8, 15, 0, DateTimeKind.Utc);
        var actual = new DateTime(2026, 4, 27, 8, 17, 30, DateTimeKind.Utc);
        var recorded = new DateTime(2026, 4, 27, 8, 18, 0, DateTimeKind.Utc);

        var log = new DelayLog
        {
            Id = 12345L,
            VehicleId = "v-1001",
            StopId = "s-001",
            TripId = "t-500",
            RouteId = "r-204",
            ScheduledArrival = scheduled,
            ActualArrival = actual,
            DelaySeconds = 150,
            RecordedAt = recorded
        };

        Assert.Equal(12345L, log.Id);
        Assert.Equal("v-1001", log.VehicleId);
        Assert.Equal("s-001", log.StopId);
        Assert.Equal("t-500", log.TripId);
        Assert.Equal("r-204", log.RouteId);
        Assert.Equal(scheduled, log.ScheduledArrival);
        Assert.Equal(actual, log.ActualArrival);
        Assert.Equal(150, log.DelaySeconds);
        Assert.Equal(recorded, log.RecordedAt);
    }

    [Fact]
    public void DelaySeconds_CanBeNegative_EarlyArrival()
    {
        var scheduled = new DateTime(2026, 4, 27, 8, 15, 0);
        var actual = new DateTime(2026, 4, 27, 8, 14, 0);

        var log = new DelayLog
        {
            ScheduledArrival = scheduled,
            ActualArrival = actual,
            DelaySeconds = -60
        };

        Assert.Equal(-60, log.DelaySeconds);
    }

    [Fact]
    public void DelaySeconds_ZeroMeansOnTime()
    {
        var log = new DelayLog { DelaySeconds = 0 };
        Assert.Equal(0, log.DelaySeconds);
    }

    [Fact]
    public void Id_UsesLong_PrimaryKey()
    {
        var log = new DelayLog { Id = long.MaxValue };
        Assert.Equal(long.MaxValue, log.Id);
    }

    [Fact]
    public void NullableFields_CanAllBeNull()
    {
        var log = new DelayLog
        {
            VehicleId = null,
            StopId = null,
            TripId = null,
            RouteId = null
        };

        Assert.Null(log.VehicleId);
        Assert.Null(log.StopId);
        Assert.Null(log.TripId);
        Assert.Null(log.RouteId);
    }
}
