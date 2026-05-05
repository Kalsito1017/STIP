using System.Text.Json;
using Moq;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;
using SofiaTransport.Infrastructure.Cache;
using StackExchange.Redis;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.Cache;

public class RedisAlertCacheTests
{
    private static Mock<IConnectionMultiplexer> CreateMockRedis(Mock<IDatabase> mockDb)
    {
        var mockRedis = new Mock<IConnectionMultiplexer>();
        mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);
        return mockRedis;
    }

    private static string SerializeAlert(ServiceAlert alert)
    {
        return JsonSerializer.Serialize(new
        {
            alert.AlertId,
            alert.HeaderText,
            alert.DescriptionText,
            alert.Url,
            alert.Cause,
            alert.Effect,
            alert.Severity,
            ActivePeriods = alert.ActivePeriods.Select(ap => new { ap.Start, ap.End }),
            InformedEntities = alert.InformedEntities.Select(ie => new { ie.AgencyId, ie.RouteId, ie.RouteType, ie.TripId, ie.StopId }),
            alert.RecordedAt
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    [Fact]
    public async Task GetAllAsync_WhenNoMembers_ReturnsEmpty()
    {
        // Arrange
        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.SetMembersAsync("alert:index", It.IsAny<CommandFlags>()))
            .ReturnsAsync(Array.Empty<RedisValue>());
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisAlertCache(mockRedis.Object);

        // Act
        var result = await cache.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDeserializedAlerts()
    {
        // Arrange
        var alert = new ServiceAlert
        {
            AlertId = "a1",
            HeaderText = "Road closed",
            DescriptionText = "Road closed for repairs",
            Cause = 1,
            Effect = 3,
            Severity = 2,
            RecordedAt = DateTime.UtcNow
        };
        var json = SerializeAlert(alert);

        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.SetMembersAsync("alert:index", It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { "a1" });
        mockDb.Setup(d => d.StringGetAsync(new RedisKey[] { "alert:a1" }, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { json });
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisAlertCache(mockRedis.Object);

        // Act
        var result = await cache.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("a1", result[0].AlertId);
        Assert.Equal("Road closed", result[0].HeaderText);
    }

    [Fact]
    public async Task GetByRouteAsync_FiltersByInformedEntities()
    {
        // Arrange
        var alert1 = new ServiceAlert
        {
            AlertId = "a1",
            HeaderText = "Alert for r-1",
            RecordedAt = DateTime.UtcNow,
            InformedEntities = { new InformedEntity { RouteId = "r-1" } }
        };
        var alert2 = new ServiceAlert
        {
            AlertId = "a2",
            HeaderText = "Alert for r-204",
            RecordedAt = DateTime.UtcNow,
            InformedEntities = { new InformedEntity { RouteId = "r-204" } }
        };
        var alert3 = new ServiceAlert
        {
            AlertId = "a3",
            HeaderText = "Alert for both",
            RecordedAt = DateTime.UtcNow,
            InformedEntities =
            {
                new InformedEntity { RouteId = "r-1" },
                new InformedEntity { RouteId = "r-204" }
            }
        };
        var json1 = SerializeAlert(alert1);
        var json2 = SerializeAlert(alert2);
        var json3 = SerializeAlert(alert3);

        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.SetMembersAsync("alert:index", It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { "a1", "a2", "a3" });
        mockDb.Setup(d => d.StringGetAsync(new RedisKey[] { "alert:a1", "alert:a2", "alert:a3" }, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { json1, json2, json3 });
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisAlertCache(mockRedis.Object);

        // Act
        var result = await cache.GetByRouteAsync("r-1");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, a => a.AlertId == "a1");
        Assert.Contains(result, a => a.AlertId == "a3");
    }

    [Fact]
    public async Task GetByRouteAsync_WhenNoMatchingRoute_ReturnsEmpty()
    {
        // Arrange
        var alert = new ServiceAlert
        {
            AlertId = "a1",
            HeaderText = "Alert",
            RecordedAt = DateTime.UtcNow,
            InformedEntities = { new InformedEntity { RouteId = "r-204" } }
        };
        var json = SerializeAlert(alert);

        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(d => d.SetMembersAsync("alert:index", It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { "a1" });
        mockDb.Setup(d => d.StringGetAsync(new RedisKey[] { "alert:a1" }, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { json });
        var mockRedis = CreateMockRedis(mockDb);
        var cache = new RedisAlertCache(mockRedis.Object);

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
        var cache = new RedisAlertCache(mockRedis.Object);
        var alert = new ServiceAlert
        {
            AlertId = "a1",
            HeaderText = "Test Alert",
            RecordedAt = DateTime.UtcNow
        };

        // Act
        await cache.SetAsync(alert);

        // Assert
        mockDb.Verify(d => d.CreateBatch(It.IsAny<object>()), Times.Once);
        mockBatch.Verify(b => b.StringSetAsync("alert:a1", It.IsAny<RedisValue>(), TimeSpan.FromSeconds(300), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
        mockBatch.Verify(b => b.SetAddAsync("alert:index", "a1", It.IsAny<CommandFlags>()), Times.Once);
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
        var cache = new RedisAlertCache(mockRedis.Object);

        // Act
        await cache.RemoveAsync("a1");

        // Assert
        mockDb.Verify(d => d.CreateBatch(It.IsAny<object>()), Times.Once);
        mockBatch.Verify(b => b.KeyDeleteAsync("alert:a1", It.IsAny<CommandFlags>()), Times.Once);
        mockBatch.Verify(b => b.SetRemoveAsync("alert:index", "a1", It.IsAny<CommandFlags>()), Times.Once);
        mockBatch.Verify(b => b.Execute(), Times.Once);
    }
}
