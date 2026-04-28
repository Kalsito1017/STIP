using Xunit;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Domain.Tests.ValueObjects;

public class CoordinatesEdgeCasesTests
{
    [Fact]
    public void Constructor_ExactlyOnLowerLatBoundary_Works()
    {
        var coords = new Coordinates(42.5001, 23.3);
        Assert.Equal(42.5001, coords.Lat);
    }

    [Fact]
    public void Constructor_ExactlyOnUpperLatBoundary_Works()
    {
        var coords = new Coordinates(42.8499, 23.3);
        Assert.Equal(42.8499, coords.Lat);
    }

    [Fact]
    public void Constructor_ExactlyOnLowerLonBoundary_Works()
    {
        var coords = new Coordinates(42.6, 23.1001);
        Assert.Equal(23.1001, coords.Lon);
    }

    [Fact]
    public void Constructor_ExactlyOnUpperLonBoundary_Works()
    {
        var coords = new Coordinates(42.6, 23.5999);
        Assert.Equal(23.5999, coords.Lon);
    }

    [Fact]
    public void Constructor_JustBelowLatBoundary_Throws()
    {
        // 42.5 is NOT thrown (strict <), but 42.4999 is below => throws
        Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinates(42.4999, 23.3));
    }

    [Fact]
    public void Constructor_JustBelowLonBoundary_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinates(42.6, 23.0999));
    }

    [Fact]
    public void Constructor_ExactlyAtLatBoundary_Works()
    {
        var coords = new Coordinates(42.5, 23.3);
        Assert.Equal(42.5, coords.Lat);
    }

    [Fact]
    public void Constructor_ExactlyAtLonBoundary_Works()
    {
        var coords = new Coordinates(42.6, 23.1);
        Assert.Equal(23.1, coords.Lon);
    }

    [Fact]
    public void Equality_SameValues_DifferentInstances_AreEqual()
    {
        var a = new Coordinates(42.6897, 23.3342);
        var b = new Coordinates(42.6897, 23.3342);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_Returns6DecimalPlaces()
    {
        var coords = new Coordinates(42.689700, 23.334200);
        var result = coords.ToString();

        Assert.Equal("42.689700,23.334200", result);
    }

    [Fact]
    public void InequalityOperator_DifferentCoordinates_ReturnsTrue()
    {
        var a = new Coordinates(42.6897, 23.3342);
        var b = new Coordinates(42.6871, 23.3186);

        Assert.True(a != b);
    }

    [Fact]
    public void InequalityOperator_SameCoordinates_ReturnsFalse()
    {
        var a = new Coordinates(42.6897, 23.3342);
        var b = new Coordinates(42.6897, 23.3342);

        Assert.False(a != b);
    }

    [Fact]
    public void LatExceptionMessage_ContainsParamName()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinates(42.0, 23.3));
        Assert.Contains("lat", ex.ParamName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LonExceptionMessage_ContainsParamName()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinates(42.6, 22.0));
        Assert.Contains("lon", ex.ParamName, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(42.5001)]
    [InlineData(42.6)]
    [InlineData(42.7)]
    [InlineData(42.8)]
    [InlineData(42.8499)]
    public void Lat_VariousValidValues_Work(double lat)
    {
        var coords = new Coordinates(lat, 23.3);
        Assert.Equal(lat, coords.Lat);
    }

    [Theory]
    [InlineData(23.1001)]
    [InlineData(23.3)]
    [InlineData(23.45)]
    [InlineData(23.5999)]
    public void Lon_VariousValidValues_Work(double lon)
    {
        var coords = new Coordinates(42.6, lon);
        Assert.Equal(lon, coords.Lon);
    }
}
