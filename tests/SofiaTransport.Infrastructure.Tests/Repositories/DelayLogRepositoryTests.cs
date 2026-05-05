using Microsoft.EntityFrameworkCore;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Infrastructure.Persistence;
using SofiaTransport.Infrastructure.Persistence.Repositories;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.Repositories;

public class DelayLogRepositoryTests
{
    private static TransportDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TransportDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TransportDbContext(options);
    }

    private static DelayLog CreateDelayLog(long id, string? routeId, string? stopId, DateTime recordedAt)
    {
        return new DelayLog
        {
            Id = id,
            RouteId = routeId,
            StopId = stopId,
            VehicleId = "v1",
            TripId = "t1",
            ScheduledArrival = recordedAt.AddMinutes(-5),
            ActualArrival = recordedAt,
            DelaySeconds = 300,
            RecordedAt = recordedAt
        };
    }

    [Fact]
    public async Task GetAllAsync_WhenLogsExist_ReturnsAllLogs()
    {
        // Arrange
        await using var db = CreateDbContext();
        var baseTime = new DateTime(2025, 5, 5, 10, 0, 0, DateTimeKind.Utc);
        db.DelayLogs.AddRange(
            CreateDelayLog(1, "r-1", "s-001", baseTime),
            CreateDelayLog(2, "r-204", "s-002", baseTime.AddMinutes(10))
        );
        await db.SaveChangesAsync();
        var repository = new DelayLogRepository(db);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoLogs_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new DelayLogRepository(db);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByRouteAsync_WithDateRange_ReturnsFilteredLogs()
    {
        // Arrange
        await using var db = CreateDbContext();
        var baseTime = new DateTime(2025, 5, 5, 10, 0, 0, DateTimeKind.Utc);
        db.DelayLogs.AddRange(
            CreateDelayLog(1, "r-1", "s-001", baseTime),
            CreateDelayLog(2, "r-1", "s-002", baseTime.AddMinutes(10)),
            CreateDelayLog(3, "r-204", "s-001", baseTime.AddMinutes(5))
        );
        await db.SaveChangesAsync();
        var repository = new DelayLogRepository(db);

        // Act
        var result = await repository.GetByRouteAsync("r-1", baseTime.AddMinutes(-1), baseTime.AddMinutes(15));

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, d => Assert.Equal("r-1", d.RouteId));
    }

    [Fact]
    public async Task GetByRouteAsync_WhenDateRangeOutOfBounds_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var baseTime = new DateTime(2025, 5, 5, 10, 0, 0, DateTimeKind.Utc);
        db.DelayLogs.Add(CreateDelayLog(1, "r-1", "s-001", baseTime));
        await db.SaveChangesAsync();
        var repository = new DelayLogRepository(db);

        // Act
        var result = await repository.GetByRouteAsync("r-1", baseTime.AddMinutes(10), baseTime.AddMinutes(20));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByStopAsync_WithDateRange_ReturnsFilteredLogs()
    {
        // Arrange
        await using var db = CreateDbContext();
        var baseTime = new DateTime(2025, 5, 5, 10, 0, 0, DateTimeKind.Utc);
        db.DelayLogs.AddRange(
            CreateDelayLog(1, "r-1", "s-001", baseTime),
            CreateDelayLog(2, "r-204", "s-001", baseTime.AddMinutes(10)),
            CreateDelayLog(3, "r-1", "s-002", baseTime.AddMinutes(5))
        );
        await db.SaveChangesAsync();
        var repository = new DelayLogRepository(db);

        // Act
        var result = await repository.GetByStopAsync("s-001", baseTime.AddMinutes(-1), baseTime.AddMinutes(15));

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, d => Assert.Equal("s-001", d.StopId));
    }

    [Fact]
    public async Task GetByStopAsync_WhenNoMatchingStop_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var baseTime = new DateTime(2025, 5, 5, 10, 0, 0, DateTimeKind.Utc);
        db.DelayLogs.Add(CreateDelayLog(1, "r-1", "s-001", baseTime));
        await db.SaveChangesAsync();
        var repository = new DelayLogRepository(db);

        // Act
        var result = await repository.GetByStopAsync("s-999", baseTime.AddMinutes(-1), baseTime.AddMinutes(15));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetForHeatmapAsync_WithDateRange_ReturnsFilteredLogs()
    {
        // Arrange
        await using var db = CreateDbContext();
        var baseTime = new DateTime(2025, 5, 5, 10, 0, 0, DateTimeKind.Utc);
        db.DelayLogs.AddRange(
            CreateDelayLog(1, "r-1", "s-001", baseTime),
            CreateDelayLog(2, "r-204", "s-002", baseTime.AddMinutes(10)),
            CreateDelayLog(3, "r-1", "s-003", baseTime.AddMinutes(20))
        );
        await db.SaveChangesAsync();
        var repository = new DelayLogRepository(db);

        // Act
        var result = await repository.GetForHeatmapAsync(baseTime, baseTime.AddMinutes(15));

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetForHeatmapAsync_WhenNoMatchingLogs_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var baseTime = new DateTime(2025, 5, 5, 10, 0, 0, DateTimeKind.Utc);
        db.DelayLogs.Add(CreateDelayLog(1, "r-1", "s-001", baseTime));
        await db.SaveChangesAsync();
        var repository = new DelayLogRepository(db);

        // Act
        var result = await repository.GetForHeatmapAsync(baseTime.AddMinutes(30), baseTime.AddMinutes(60));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByDateAsync_ReturnsLogsForSpecifiedDate()
    {
        // Arrange
        await using var db = CreateDbContext();
        var date = new DateTime(2025, 5, 5, 0, 0, 0, DateTimeKind.Utc);
        db.DelayLogs.AddRange(
            CreateDelayLog(1, "r-1", "s-001", date.AddHours(10)),
            CreateDelayLog(2, "r-204", "s-002", date.AddHours(14)),
            CreateDelayLog(3, "r-1", "s-003", date.AddDays(1).AddHours(1))
        );
        await db.SaveChangesAsync();
        var repository = new DelayLogRepository(db);

        // Act
        var result = await repository.GetByDateAsync(date);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, d => Assert.True(d.RecordedAt >= date && d.RecordedAt < date.AddDays(1)));
    }

    [Fact]
    public async Task GetByDateAsync_WhenNoLogsForDate_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var date = new DateTime(2025, 5, 5, 0, 0, 0, DateTimeKind.Utc);
        db.DelayLogs.Add(CreateDelayLog(1, "r-1", "s-001", date));
        await db.SaveChangesAsync();
        var repository = new DelayLogRepository(db);

        // Act
        var result = await repository.GetByDateAsync(date.AddDays(1));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddAsync_AddsLog_AndReturnsIt()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new DelayLogRepository(db);
        var log = CreateDelayLog(100, "r-1", "s-001", DateTime.UtcNow);

        // Act
        var result = await repository.AddAsync(log);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Id);

        // Verify persisted
        var persisted = await db.DelayLogs.FindAsync(100L);
        Assert.NotNull(persisted);
    }
}
