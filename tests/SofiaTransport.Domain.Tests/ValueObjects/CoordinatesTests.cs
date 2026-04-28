using Xunit;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Domain.Tests.ValueObjects;

public class CoordinatesTests
{
    [Fact]
    public void Constructor_ValidSofiaCoordinates_CreatesInstance()
    {
        var coords = new Coordinates(42.6977, 23.3219);
        Assert.Equal(42.6977, coords.Lat);
        Assert.Equal(23.3219, coords.Lon);
    }

    [Theory]
    [InlineData(42.0, 23.3)]   // lat too low
    [InlineData(43.0, 23.3)]   // lat too high
    [InlineData(42.6, 22.5)]   // lon too low
    [InlineData(42.6, 24.0)]   // lon too high
    public void Constructor_InvalidCoordinates_ThrowsArgumentOutOfRange(double lat, double lon)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinates(lat, lon));
    }

    [Fact]
    public void Constructor_AtLatBoundary_Works()
    {
        var min = new Coordinates(42.5001, 23.3);
        var max = new Coordinates(42.8499, 23.3);
        Assert.NotNull(min);
        Assert.NotNull(max);
    }

    [Fact]
    public void ToString_ReturnsFormattedCoordinates()
    {
        var coords = new Coordinates(42.697700, 23.321900);
        var result = coords.ToString();
        Assert.Contains("42.", result);
        Assert.Contains("23.", result);
    }

    [Fact]
    public void Equality_TwoIdenticalCoordinates_AreEqual()
    {
        var a = new Coordinates(42.6977, 23.3219);
        var b = new Coordinates(42.6977, 23.3219);
        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Equality_DifferentCoordinates_AreNotEqual()
    {
        var a = new Coordinates(42.6977, 23.3219);
        var b = new Coordinates(42.6939, 23.3451);
        Assert.NotEqual(a, b);
    }
}
