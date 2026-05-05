using Microsoft.EntityFrameworkCore;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Infrastructure.Persistence;
using SofiaTransport.Infrastructure.Persistence.Repositories;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.Repositories;

public class ShapeRepositoryTests
{
    private static TransportDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TransportDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TransportDbContext(options);
    }

    [Fact]
    public async Task GetByRouteIdAsync_ReturnsShapesOrderedBySequence()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-1", ShortName = "1" });
        db.Shapes.AddRange(
            new Shape { Id = 1, RouteId = "r-1", Sequence = 3, Lat = 42.70, Lon = 23.35 },
            new Shape { Id = 2, RouteId = "r-1", Sequence = 1, Lat = 42.69, Lon = 23.33 },
            new Shape { Id = 3, RouteId = "r-1", Sequence = 2, Lat = 42.71, Lon = 23.34 },
            new Shape { Id = 4, RouteId = "r-204", Sequence = 1, Lat = 42.68, Lon = 23.32 }
        );
        await db.SaveChangesAsync();
        var repository = new ShapeRepository(db);

        // Act
        var result = await repository.GetByRouteIdAsync("r-1");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].Sequence);
        Assert.Equal(2, result[1].Sequence);
        Assert.Equal(3, result[2].Sequence);
    }

    [Fact]
    public async Task GetByRouteIdAsync_WhenRouteHasNoShapes_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-1", ShortName = "1" });
        db.Shapes.Add(new Shape { Id = 1, RouteId = "r-204", Sequence = 1, Lat = 42.68, Lon = 23.32 });
        await db.SaveChangesAsync();
        var repository = new ShapeRepository(db);

        // Act
        var result = await repository.GetByRouteIdAsync("r-1");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByRouteIdAsync_WhenRouteDoesNotExist_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new ShapeRepository(db);

        // Act
        var result = await repository.GetByRouteIdAsync("r-nonexistent");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllGroupedByRouteAsync_ReturnsShapesOrderedByRouteIdThenSequence()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.AddRange(
            new Route { RouteId = "r-1", ShortName = "1" },
            new Route { RouteId = "r-204", ShortName = "204" }
        );
        db.Shapes.AddRange(
            new Shape { Id = 1, RouteId = "r-204", Sequence = 2, Lat = 42.69, Lon = 23.33 },
            new Shape { Id = 2, RouteId = "r-1", Sequence = 1, Lat = 42.70, Lon = 23.35 },
            new Shape { Id = 3, RouteId = "r-204", Sequence = 1, Lat = 42.68, Lon = 23.32 },
            new Shape { Id = 4, RouteId = "r-1", Sequence = 2, Lat = 42.71, Lon = 23.34 }
        );
        await db.SaveChangesAsync();
        var repository = new ShapeRepository(db);

        // Act
        var result = await repository.GetAllGroupedByRouteAsync();

        // Assert
        Assert.Equal(4, result.Count);
        // r-1 comes before r-204 (alphabetically)
        Assert.Equal("r-1", result[0].RouteId);
        Assert.Equal("r-1", result[1].RouteId);
        Assert.Equal("r-204", result[2].RouteId);
        Assert.Equal("r-204", result[3].RouteId);
        // Within each route, ordered by sequence
        Assert.Equal(1, result[0].Sequence);
        Assert.Equal(2, result[1].Sequence);
        Assert.Equal(1, result[2].Sequence);
        Assert.Equal(2, result[3].Sequence);
    }

    [Fact]
    public async Task GetAllGroupedByRouteAsync_WhenNoShapes_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new ShapeRepository(db);

        // Act
        var result = await repository.GetAllGroupedByRouteAsync();

        // Assert
        Assert.Empty(result);
    }
}
