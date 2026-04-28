using Xunit;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Domain.Tests.ValueObjects;

public class CoordinatesExtendedTests
{
    [Fact]
    public void Equality_VeryCloseButNotEqual_AreNotEqual()
    {
        var a = new Coordinates(42.697700001, 23.321900001);
        var b = new Coordinates(42.697700002, 23.321900002);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equality_SameTo15DecimalPlaces_AreEqual()
    {
        // Records use value equality; doubles that are exactly equal are equal
        var a = new Coordinates(42.697700000000001, 23.321900000000001);
        var b = new Coordinates(42.697700000000001, 23.321900000000001);

        Assert.Equal(a, b);
    }

    [Fact]
    public void SofiaCityCenterCoordinates_Valid()
    {
        // Sofia city center approximate coordinates
        var coords = new Coordinates(42.697708, 23.321868);

        Assert.Equal(42.697708, coords.Lat);
        Assert.Equal(23.321868, coords.Lon);
    }

    [Fact]
    public void Equality_WithExactlySofiaCityCenter_Equal()
    {
        var a = new Coordinates(42.697708, 23.321868);
        var b = new Coordinates(42.697708, 23.321868);

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void DifferentLat_SameLon_AreNotEqual()
    {
        var a = new Coordinates(42.6977, 23.3219);
        var b = new Coordinates(42.6978, 23.3219);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void SameLat_DifferentLon_AreNotEqual()
    {
        var a = new Coordinates(42.6977, 23.3219);
        var b = new Coordinates(42.6977, 23.3220);

        Assert.NotEqual(a, b);
    }
}
