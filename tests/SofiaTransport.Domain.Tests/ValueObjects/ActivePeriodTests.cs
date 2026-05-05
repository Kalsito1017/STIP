using Xunit;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Domain.Tests.ValueObjects;

public class ActivePeriodTests
{
    [Fact]
    public void Constructor_DefaultValues_AreNull()
    {
        var ap = new ActivePeriod();
        Assert.Null(ap.Start);
        Assert.Null(ap.End);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var ap = new ActivePeriod
        {
            Start = 1690000000,
            End = 1690003600
        };

        Assert.Equal(1690000000, ap.Start);
        Assert.Equal(1690003600, ap.End);
    }

    [Fact]
    public void Start_CanBeNull()
    {
        var ap = new ActivePeriod { Start = null, End = 2000 };
        Assert.Null(ap.Start);
        Assert.Equal(2000, ap.End);
    }

    [Fact]
    public void End_CanBeNull()
    {
        var ap = new ActivePeriod { Start = 1000, End = null };
        Assert.Equal(1000, ap.Start);
        Assert.Null(ap.End);
    }

    [Fact]
    public void Equality_TwoIdenticalPeriods_AreEqual()
    {
        var a = new ActivePeriod { Start = 1000, End = 2000 };
        var b = new ActivePeriod { Start = 1000, End = 2000 };
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentPeriods_AreNotEqual()
    {
        var a = new ActivePeriod { Start = 1000, End = 2000 };
        var b = new ActivePeriod { Start = 1000, End = 3000 };
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void BothNull_AreEqual()
    {
        var a = new ActivePeriod { Start = null, End = null };
        var b = new ActivePeriod { Start = null, End = null };
        Assert.Equal(a, b);
    }
}
