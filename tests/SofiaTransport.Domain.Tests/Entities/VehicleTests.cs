using Xunit;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Domain.Tests.Entities;

public class VehicleTests
{
    [Fact]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        var vehicle = new Vehicle();
        Assert.Equal(string.Empty, vehicle.VehicleId);
        Assert.Null(vehicle.RouteId);
        Assert.Null(vehicle.TripId);
        Assert.Null(vehicle.Location);
        Assert.Equal(0f, vehicle.Bearing);
        Assert.Equal(0f, vehicle.Speed);
        Assert.Equal(default(DateTime), vehicle.RecordedAt);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var location = new Coordinates(42.69, 23.33);
        var recordedAt = new DateTime(2026, 4, 27, 14, 30, 0, DateTimeKind.Utc);

        var vehicle = new Vehicle
        {
            VehicleId = "v-1001",
            RouteId = "r-204",
            TripId = "t-500",
            Location = location,
            Bearing = 45.5f,
            Speed = 32.0f,
            RecordedAt = recordedAt
        };

        Assert.Equal("v-1001", vehicle.VehicleId);
        Assert.Equal("r-204", vehicle.RouteId);
        Assert.Equal("t-500", vehicle.TripId);
        Assert.Equal(42.69, vehicle.Location.Lat);
        Assert.Equal(23.33, vehicle.Location.Lon);
        Assert.Equal(45.5f, vehicle.Bearing);
        Assert.Equal(32.0f, vehicle.Speed);
        Assert.Equal(recordedAt, vehicle.RecordedAt);
    }

    [Fact]
    public void RouteId_CanBeNull()
    {
        var vehicle = new Vehicle { VehicleId = "v-1", RouteId = null };
        Assert.Null(vehicle.RouteId);
    }

    [Fact]
    public void TripId_CanBeNull()
    {
        var vehicle = new Vehicle { VehicleId = "v-1", TripId = null };
        Assert.Null(vehicle.TripId);
    }

    [Fact]
    public void Bearing_CanBeNegative()
    {
        var vehicle = new Vehicle { VehicleId = "v-1", Bearing = -45.0f };
        Assert.Equal(-45.0f, vehicle.Bearing);
    }

    [Fact]
    public void Bearing_CanExceed360()
    {
        var vehicle = new Vehicle { VehicleId = "v-1", Bearing = 400.0f };
        Assert.Equal(400.0f, vehicle.Bearing);
    }

    [Fact]
    public void Speed_CanBeZero()
    {
        var vehicle = new Vehicle { VehicleId = "v-1", Speed = 0.0f };
        Assert.Equal(0.0f, vehicle.Speed);
    }
}
