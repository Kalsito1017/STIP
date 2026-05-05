using System.Text.Json;
using Moq;
using NetTopologySuite.Geometries;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;
using SofiaTransport.Infrastructure.Cache;
using StackExchange.Redis;
using Xunit;
using Coordinates = SofiaTransport.Domain.ValueObjects.Coordinates;

namespace SofiaTransport.Infrastructure.Tests.Cache;

public class RedisVehicleCacheTests
{
    private static Mock<IConnectionMultiplexer> CreateMockRedis(Mock<IDatabase> mockDb)
    {
        var mockRedis = new Mock<IConnectionMultiplexer>();
        mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);
        return mockRedis;
    }

    private static string SerializeVehicle(Vehicle v)
    {
        return JsonSerializer.Serialize(new
        {
            vehicleId = v.VehicleId,
            routeId = v.RouteId,
            tripId = v.TripId,
            lat = v.Location.Lat,
            lon = v.Location.Lon,
            bearing = v.Bearing,
            speed = v.Speed,
            recordedAt = v.RecordedAt
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    [Fact]
    public async Task GetAllAsync_WhenNoMembers_ReturnsEmpty()
    {
        // Arrange
        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.SetMembersAsync("vehicle:index", It.IsAny<CommandFlags>()))
            .ReturnsAsync(Array.Empty<RedisValue>());
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisVehicleCache(mockRedis.Object);

        // Act
        var result = await cache.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDeserializedVehicles()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            VehicleId = "v1",
            RouteId = "r-1",
            TripId = "t1",
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 },
            Bearing = 90f,
            Speed = 40f,
            RecordedAt = DateTime.UtcNow
        };
        var json = SerializeVehicle(vehicle);

        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.SetMembersAsync("vehicle:index", It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { "v1" });
        mockDb.Setup(d => d.StringGetAsync(new RedisKey[] { "vehicle:v1" }, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { json });
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisVehicleCache(mockRedis.Object);

        // Act
        var result = await cache.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("v1", result[0].VehicleId);
        Assert.Equal("r-1", result[0].RouteId);
    }

    [Fact]
    public async Task GetAllAsync_SkipsNullValuesAndRemovesStaleIndex()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            VehicleId = "v1",
            RouteId = "r-1",
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 },
            RecordedAt = DateTime.UtcNow
        };
        var json = SerializeVehicle(vehicle);

        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.SetMembersAsync("vehicle:index", It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { "v1", "v2" });
        // v2 has null value
        mockDb.Setup(d => d.StringGetAsync(new RedisKey[] { "vehicle:v1", "vehicle:v2" }, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { json, RedisValue.Null });
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisVehicleCache(mockRedis.Object);

        // Act
        var result = await cache.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("v1", result[0].VehicleId);

        // Verify stale index cleanup was called
        mockDb.Verify(d => d.SetRemoveAsync("vehicle:index", "v2", It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task GetByRouteAsync_FiltersVehiclesByRoute()
    {
        // Arrange
        var v1 = new Vehicle
        {
            VehicleId = "v1",
            RouteId = "r-1",
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 },
            RecordedAt = DateTime.UtcNow
        };
        var v2 = new Vehicle
        {
            VehicleId = "v2",
            RouteId = "r-204",
            Location = new Coordinates(42.68, 23.32),
            Geometry = new Point(23.32, 42.68) { SRID = 4326 },
            RecordedAt = DateTime.UtcNow
        };
        var json1 = SerializeVehicle(v1);
        var json2 = SerializeVehicle(v2);

        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.SetMembersAsync("vehicle:index", It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { "v1", "v2" });
        mockDb.Setup(d => d.StringGetAsync(new RedisKey[] { "vehicle:v1", "vehicle:v2" }, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { json1, json2 });
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisVehicleCache(mockRedis.Object);

        // Act
        var result = await cache.GetByRouteAsync("r-204");

        // Assert
        Assert.Single(result);
        Assert.Equal("v2", result[0].VehicleId);
    }

    [Fact]
    public async Task GetAsync_ReturnsVehicleById()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            VehicleId = "v1",
            RouteId = "r-1",
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 },
            RecordedAt = DateTime.UtcNow
        };
        var json = SerializeVehicle(vehicle);

        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.StringGetAsync("vehicle:v1", It.IsAny<CommandFlags>()))
            .ReturnsAsync(json);
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisVehicleCache(mockRedis.Object);

        // Act
        var result = await cache.GetAsync("v1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("v1", result!.VehicleId);
    }

    [Fact]
    public async Task GetAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.StringGetAsync("vehicle:v-nope", It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisVehicleCache(mockRedis.Object);

        // Act
        var result = await cache.GetAsync("v-nope");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_CallsStringSetAndSetAddInBatch()
    {
        // Arrange
        var mockBatch = new Mock<IBatch>();
        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.CreateBatch(It.IsAny<object>())).Returns(mockBatch.Object);

        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisVehicleCache(mockRedis.Object);
        var vehicle = new Vehicle
        {
            VehicleId = "v1",
            RouteId = "r-1",
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 },
            RecordedAt = DateTime.UtcNow
        };

        // Act
        await cache.SetAsync(vehicle);

        // Assert
        mockDb.Verify(d => d.CreateBatch(It.IsAny<object>()), Times.Once);
        mockBatch.Verify(b => b.Execute(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_CallsKeyDeleteAndSetRemoveInBatch()
    {
        // Arrange
        var mockBatch = new Mock<IBatch>();
        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.CreateBatch(It.IsAny<object>())).Returns(mockBatch.Object);

        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisVehicleCache(mockRedis.Object);

        // Act
        await cache.RemoveAsync("v1");

        // Assert
        mockDb.Verify(d => d.CreateBatch(It.IsAny<object>()), Times.Once);
        mockBatch.Verify(b => b.KeyDeleteAsync("vehicle:v1", It.IsAny<CommandFlags>()), Times.Once);
        mockBatch.Verify(b => b.SetRemoveAsync("vehicle:index", "v1", It.IsAny<CommandFlags>()), Times.Once);
        mockBatch.Verify(b => b.Execute(), Times.Once);
    }
}
