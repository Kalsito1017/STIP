using Xunit;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Domain.Tests.Entities;

public class ServiceAlertTests
{
    [Fact]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        var alert = new ServiceAlert();
        Assert.Equal(string.Empty, alert.AlertId);
        Assert.Equal(string.Empty, alert.HeaderText);
        Assert.Null(alert.DescriptionText);
        Assert.Null(alert.Url);
        Assert.Equal(0, alert.Cause);
        Assert.Equal(0, alert.Effect);
        Assert.Null(alert.Severity);
        Assert.NotNull(alert.ActivePeriods);
        Assert.Empty(alert.ActivePeriods);
        Assert.NotNull(alert.InformedEntities);
        Assert.Empty(alert.InformedEntities);
        Assert.Equal(default, alert.RecordedAt);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var alert = new ServiceAlert
        {
            AlertId = "alert-001",
            HeaderText = "Route delay",
            DescriptionText = "Delays due to construction",
            Url = "https://example.com",
            Cause = 9,
            Effect = 3,
            Severity = 2,
            RecordedAt = new DateTime(2026, 5, 1, 12, 0, 0)
        };

        Assert.Equal("alert-001", alert.AlertId);
        Assert.Equal("Route delay", alert.HeaderText);
        Assert.Equal("Delays due to construction", alert.DescriptionText);
        Assert.Equal("https://example.com", alert.Url);
        Assert.Equal(9, alert.Cause);
        Assert.Equal(3, alert.Effect);
        Assert.Equal(2, alert.Severity);
        Assert.Equal(new DateTime(2026, 5, 1, 12, 0, 0), alert.RecordedAt);
    }

    [Fact]
    public void ActivePeriods_Collection_IsMutable()
    {
        var alert = new ServiceAlert();
        var period = new ActivePeriod { Start = 1000, End = 2000 };
        alert.ActivePeriods.Add(period);

        Assert.Single(alert.ActivePeriods);
        Assert.Equal(1000, alert.ActivePeriods[0].Start);
        Assert.Equal(2000, alert.ActivePeriods[0].End);
    }

    [Fact]
    public void InformedEntities_Collection_IsMutable()
    {
        var alert = new ServiceAlert();
        var entity = new InformedEntity { RouteId = "r-204", StopId = "s-001" };
        alert.InformedEntities.Add(entity);

        Assert.Single(alert.InformedEntities);
        Assert.Equal("r-204", alert.InformedEntities[0].RouteId);
        Assert.Equal("s-001", alert.InformedEntities[0].StopId);
    }

    [Fact]
    public void DescriptionText_CanBeNull()
    {
        var alert = new ServiceAlert { DescriptionText = null };
        Assert.Null(alert.DescriptionText);
    }

    [Fact]
    public void Url_CanBeNull()
    {
        var alert = new ServiceAlert { Url = null };
        Assert.Null(alert.Url);
    }

    [Fact]
    public void Severity_CanBeNull()
    {
        var alert = new ServiceAlert { Severity = null };
        Assert.Null(alert.Severity);
    }
}
