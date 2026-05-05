using Xunit;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Domain.Tests.ValueObjects;

public class InformedEntityTests
{
    [Fact]
    public void Constructor_DefaultValues_AreNull()
    {
        var ie = new InformedEntity();
        Assert.Null(ie.AgencyId);
        Assert.Null(ie.RouteId);
        Assert.Null(ie.RouteType);
        Assert.Null(ie.TripId);
        Assert.Null(ie.StopId);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var ie = new InformedEntity
        {
            AgencyId = "agency-1",
            RouteId = "r-204",
            RouteType = 3,
            TripId = "t-001",
            StopId = "s-001"
        };

        Assert.Equal("agency-1", ie.AgencyId);
        Assert.Equal("r-204", ie.RouteId);
        Assert.Equal(3, ie.RouteType);
        Assert.Equal("t-001", ie.TripId);
        Assert.Equal("s-001", ie.StopId);
    }

    [Fact]
    public void Equality_TwoIdenticalEntities_AreEqual()
    {
        var a = new InformedEntity { RouteId = "r-204", StopId = "s-001" };
        var b = new InformedEntity { RouteId = "r-204", StopId = "s-001" };
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentEntities_AreNotEqual()
    {
        var a = new InformedEntity { RouteId = "r-204" };
        var b = new InformedEntity { RouteId = "r-285" };
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void AllPropertiesCanBeNull()
    {
        var ie = new InformedEntity
        {
            AgencyId = null,
            RouteId = null,
            RouteType = null,
            TripId = null,
            StopId = null
        };

        Assert.Null(ie.AgencyId);
        Assert.Null(ie.RouteId);
        Assert.Null(ie.RouteType);
        Assert.Null(ie.TripId);
        Assert.Null(ie.StopId);
    }
}
