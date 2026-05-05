using System.Text.Json;
using Moq;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Infrastructure.Cache;
using StackExchange.Redis;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.Cache;

public class RedisTripUpdateCacheTests
{
    private static Mock<IConnectionMultiplexer> CreateMockRedis(Mock<IDatabase> mockDb)
    {
        var mockRedis = new Mock<IConnectionMultiplexer>();
        mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);
        return mockRedis;
    }

    private static string SerializeTripUpdate(TripUpdate tu)
    {
        return JsonSerializer.Serialize(new
        {
            tu.TripId,
            tu.RouteId,
            tu.StartTime,
            tu.StartDate,
            tu.ScheduleRelationship,
            tu.VehicleId,
            StopTimeUpdates = tu.StopTimeUpdates.Select(stu => new
            {
                stu.StopSequence,
                stu.StopId,
                stu.ArrivalDelay,
                stu.ArrivalTime,
                stu.DepartureDelay,
                stu.DepartureTime,
                stu.ScheduleRelationship
            }),
            tu.RecordedAt
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    [Fact]
    public async Task GetAllAsync_WhenNoMembers_ReturnsEmpty()
    {
        // Arrange
        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.SetMembersAsync("tripupdate:index", It.IsAny<CommandFlags>()))
            .ReturnsAsync(Array.Empty<RedisValue>());
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisTripUpdateCache(mockRedis.Object);

        // Act
        var result = await cache.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDeserializedTripUpdates()
    {
        // Arrange
        var tu = new TripUpdate
        {
            TripId = "t1",
            RouteId = "r-1",
            StartTime = "08:00:00",
            ScheduleRelationship = 0,
            VehicleId = "v1",
            RecordedAt = DateTime.UtcNow,
            StopTimeUpdates =
            {
                new StopTimeUpdate
                {
                    StopSequence = 1,
                    StopId = "s-001",
                    ArrivalDelay = 120,
                    ArrivalTime = 28800,
                    ScheduleRelationship = 0
                }
            }
        };
        var json = SerializeTripUpdate(tu);

        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.SetMembersAsync("tripupdate:index", It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { "t1" });
        mockDb.Setup(d => d.StringGetAsync(new RedisKey[] { "tripupdate:t1" }, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { json });
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisTripUpdateCache(mockRedis.Object);

        // Act
        var result = await cache.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("t1", result[0].TripId);
        Assert.Equal("r-1", result[0].RouteId);
        Assert.Single(result[0].StopTimeUpdates);
        Assert.Equal(120, result[0].StopTimeUpdates[0].ArrivalDelay);
    }

    [Fact]
    public async Task GetByRouteAsync_FiltersByRouteId()
    {
        // Arrange
        var tu1 = new TripUpdate
        {
            TripId = "t1",
            RouteId = "r-1",
            RecordedAt = DateTime.UtcNow
        };
        var tu2 = new TripUpdate
        {
            TripId = "t2",
            RouteId = "r-204",
            RecordedAt = DateTime.UtcNow
        };
        var json1 = SerializeTripUpdate(tu1);
        var json2 = SerializeTripUpdate(tu2);

        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.SetMembersAsync("tripupdate:index", It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { "t1", "t2" });
        mockDb.Setup(d => d.StringGetAsync(new RedisKey[] { "tripupdate:t1", "tripupdate:t2" }, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { json1, json2 });
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisTripUpdateCache(mockRedis.Object);

        // Act
        var result = await cache.GetByRouteAsync("r-204");

        // Assert
        Assert.Single(result);
        Assert.Equal("t2", result[0].TripId);
    }

    [Fact]
    public async Task GetByRouteAsync_WhenNoMatchingRoute_ReturnsEmpty()
    {
        // Arrange
        var tu = new TripUpdate
        {
            TripId = "t1",
            RouteId = "r-1",
            RecordedAt = DateTime.UtcNow
        };
        var json = SerializeTripUpdate(tu);

        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.SetMembersAsync("tripupdate:index", It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { "t1" });
        mockDb.Setup(d => d.StringGetAsync(new RedisKey[] { "tripupdate:t1" }, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { json });
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisTripUpdateCache(mockRedis.Object);

        // Act
        var result = await cache.GetByRouteAsync("r-999");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SetAsync_CallsStringSetAndSetAddInBatch()
    {
        // Arrange
        var mockBatch = new Mock<IBatch>();
        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.CreateBatch(It.IsAny<object>())).Returns(mockBatch.Object);

        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisTripUpdateCache(mockRedis.Object);
        var tu = new TripUpdate
        {
            TripId = "t1",
            RouteId = "r-1",
            RecordedAt = DateTime.UtcNow
        };

        // Act
        await cache.SetAsync(tu);

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
        var cache = new RedisTripUpdateCache(mockRedis.Object);

        // Act
        await cache.RemoveAsync("t1");

        // Assert
        mockDb.Verify(d => d.CreateBatch(It.IsAny<object>()), Times.Once);
        mockBatch.Verify(b => b.KeyDeleteAsync("tripupdate:t1", It.IsAny<CommandFlags>()), Times.Once);
        mockBatch.Verify(b => b.SetRemoveAsync("tripupdate:index", "t1", It.IsAny<CommandFlags>()), Times.Once);
        mockBatch.Verify(b => b.Execute(), Times.Once);
    }
}
