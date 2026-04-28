using Xunit;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Domain.Tests.Entities;

public class StopTests
{
    [Fact]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        var stop = new Stop();
        Assert.Equal(string.Empty, stop.StopId);
        Assert.Equal(string.Empty, stop.StopName);
        Assert.Null(stop.Location); // not initialized by default
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var location = new Coordinates(42.6897, 23.3342);
        var stop = new Stop
        {
            StopId = "s-001",
            StopName = "Orlov Most",
            Location = location
        };

        Assert.Equal("s-001", stop.StopId);
        Assert.Equal("Orlov Most", stop.StopName);
        Assert.Equal(location, stop.Location);
        Assert.Equal(42.6897, stop.Location.Lat);
        Assert.Equal(23.3342, stop.Location.Lon);
    }

    [Fact]
    public void Location_UsesCoordinatesValueObject()
    {
        var stop = new Stop
        {
            StopId = "s-002",
            StopName = "NDK",
            Location = new Coordinates(42.6871, 23.3186)
        };

        Assert.IsType<Coordinates>(stop.Location);
        Assert.Equal(42.6871, stop.Location.Lat);
        Assert.Equal(23.3186, stop.Location.Lon);
    }

    [Fact]
    public void StopId_CanContainHyphens()
    {
        var stop = new Stop { StopId = "s-abc-123", StopName = "Test" };
        Assert.Equal("s-abc-123", stop.StopId);
    }

    [Fact]
    public void StopName_SupportsBulgarianCharacters()
    {
        var stop = new Stop
        {
            StopId = "s-003",
            StopName = "Ж.К. Младост 1",
            Location = new Coordinates(42.65, 23.38)
        };

        Assert.Equal("Ж.К. Младост 1", stop.StopName);
    }
}
