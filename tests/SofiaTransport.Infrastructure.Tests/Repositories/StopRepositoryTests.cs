using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;
using SofiaTransport.Infrastructure.Persistence;
using SofiaTransport.Infrastructure.Persistence.Repositories;
using Xunit;
using Coordinates = SofiaTransport.Domain.ValueObjects.Coordinates;

namespace SofiaTransport.Infrastructure.Tests.Repositories;

public class StopRepositoryTests
{
    private static TransportDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TransportDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TransportDbContext(options);
    }

    private static Stop CreateStop(string stopId, string stopName, double lat, double lon)
    {
        return new Stop
        {
            StopId = stopId,
            StopName = stopName,
            Location = new Coordinates(lat, lon),
            Geometry = new Point(lon, lat) { SRID = 4326 }
        };
    }

    [Fact]
    public async Task GetAllAsync_WhenStopsExist_ReturnsAllStops()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Stops.AddRange(
            CreateStop("s-001", "Orlov Most", 42.6897, 23.3342),
            CreateStop("s-002", "Sofia University", 42.6939, 23.3451)
        );
        await db.SaveChangesAsync();
        var repository = new StopRepository(db);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.StopId == "s-001");
        Assert.Contains(result, s => s.StopId == "s-002");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoStops_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new StopRepository(db);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenStopExists_ReturnsStop()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Stops.Add(CreateStop("s-001", "Orlov Most", 42.6897, 23.3342));
        await db.SaveChangesAsync();
        var repository = new StopRepository(db);

        // Act
        var result = await repository.GetByIdAsync("s-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("s-001", result!.StopId);
        Assert.Equal("Orlov Most", result.StopName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenStopDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new StopRepository(db);

        // Act
        var result = await repository.GetByIdAsync("s-nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCountAsync_WhenStopsExist_ReturnsCorrectCount()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Stops.AddRange(
            CreateStop("s-001", "Orlov Most", 42.6897, 23.3342),
            CreateStop("s-002", "Sofia University", 42.6939, 23.3451),
            CreateStop("s-003", "NDK", 42.6871, 23.3186)
        );
        await db.SaveChangesAsync();
        var repository = new StopRepository(db);

        // Act
        var result = await repository.GetCountAsync();

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenAllIdsMatch_ReturnsMatchingStops()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Stops.AddRange(
            CreateStop("s-001", "Orlov Most", 42.6897, 23.3342),
            CreateStop("s-002", "Sofia University", 42.6939, 23.3451),
            CreateStop("s-003", "NDK", 42.6871, 23.3186)
        );
        await db.SaveChangesAsync();
        var repository = new StopRepository(db);

        // Act
        var result = await repository.GetByIdsAsync(new[] { "s-001", "s-003" });

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.StopId == "s-001");
        Assert.Contains(result, s => s.StopId == "s-003");
    }

    [Fact]
    public async Task GetByIdsAsync_WhenEmptyList_ReturnsEmptyArray()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Stops.Add(CreateStop("s-001", "Orlov Most", 42.6897, 23.3342));
        await db.SaveChangesAsync();
        var repository = new StopRepository(db);

        // Act
        var result = await repository.GetByIdsAsync(Array.Empty<string>());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenSomeIdsDoNotMatch_ReturnsOnlyMatching()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Stops.AddRange(
            CreateStop("s-001", "Orlov Most", 42.6897, 23.3342),
            CreateStop("s-002", "Sofia University", 42.6939, 23.3451)
        );
        await db.SaveChangesAsync();
        var repository = new StopRepository(db);

        // Act
        var result = await repository.GetByIdsAsync(new[] { "s-002", "s-999" });

        // Assert
        Assert.Single(result);
        Assert.Equal("s-002", result[0].StopId);
    }

    [Fact]
    public async Task AddAsync_AddsStop_AndReturnsIt()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new StopRepository(db);
        var stop = CreateStop("s-new", "New Stop", 42.7000, 23.3500);

        // Act
        var result = await repository.AddAsync(stop);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("s-new", result.StopId);

        // Verify persisted
        var persisted = await db.Stops.FindAsync("s-new");
        Assert.NotNull(persisted);
        Assert.Equal("New Stop", persisted!.StopName);
    }
}
