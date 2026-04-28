using Xunit;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Domain.Tests.Entities;

public class DelayLogEdgeCasesTests
{
    [Fact]
    public void DelaySeconds_CanBeNegative_EarlyArrival()
    {
        var log = new DelayLog
        {
            DelaySeconds = -300,
            ScheduledArrival = new DateTime(2026, 4, 27, 8, 15, 0),
            ActualArrival = new DateTime(2026, 4, 27, 8, 10, 0)
        };

        Assert.Equal(-300, log.DelaySeconds);
    }

    [Fact]
    public void DelaySeconds_CanBeVeryLarge_HoursOfDelay()
    {
        var log = new DelayLog
        {
            DelaySeconds = 7200 // 2 hours
        };

        Assert.Equal(7200, log.DelaySeconds);
    }

    [Fact]
    public void DelaySeconds_CanBeExtreme()
    {
        var log = new DelayLog
        {
            DelaySeconds = 43200 // 12 hours
        };

        Assert.Equal(43200, log.DelaySeconds);
    }

    [Fact]
    public void NullVehicleId_Allowed()
    {
        var log = new DelayLog { VehicleId = null };
        Assert.Null(log.VehicleId);
    }

    [Fact]
    public void NullStopId_Allowed()
    {
        var log = new DelayLog { StopId = null };
        Assert.Null(log.StopId);
    }

    [Fact]
    public void NullTripId_Allowed()
    {
        var log = new DelayLog { TripId = null };
        Assert.Null(log.TripId);
    }

    [Fact]
    public void RecordedAt_DefaultValue_IsDefaultDateTime()
    {
        var log = new DelayLog();
        Assert.Equal(default(DateTime), log.RecordedAt);
    }

    [Fact]
    public void RecordedAt_CanBeSetToUtcNow()
    {
        var now = DateTime.UtcNow;
        var log = new DelayLog { RecordedAt = now };
        Assert.Equal(now, log.RecordedAt);
    }
}
