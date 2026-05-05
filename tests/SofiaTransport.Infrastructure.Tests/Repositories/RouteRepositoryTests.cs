using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;
using SofiaTransport.Infrastructure.Persistence;
using SofiaTransport.Infrastructure.Persistence.Repositories;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.Repositories;

public class RouteRepositoryTests
{
    private static TransportDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TransportDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TransportDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_WhenRoutesExist_ReturnsAllRoutes()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.AddRange(
            new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Metro },
            new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus }
        );
        await db.SaveChangesAsync();
        var repository = new RouteRepository(db);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.RouteId == "r-1");
        Assert.Contains(result, r => r.RouteId == "r-204");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoRoutes_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new RouteRepository(db);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRouteExists_ReturnsRoute()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus });
        await db.SaveChangesAsync();
        var repository = new RouteRepository(db);

        // Act
        var result = await repository.GetByIdAsync("r-204");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("r-204", result!.RouteId);
        Assert.Equal("204", result.ShortName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRouteDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new RouteRepository(db);

        // Act
        var result = await repository.GetByIdAsync("r-nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCountAsync_WhenRoutesExist_ReturnsCorrectCount()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.AddRange(
            new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Metro },
            new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus },
            new Route { RouteId = "r-tram-1", ShortName = "1", Type = TransitType.Tram }
        );
        await db.SaveChangesAsync();
        var repository = new RouteRepository(db);

        // Act
        var result = await repository.GetCountAsync();

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task GetCountAsync_WhenNoRoutes_ReturnsZero()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new RouteRepository(db);

        // Act
        var result = await repository.GetCountAsync();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetByTypeAsync_WhenMatchingRoutes_ReturnsFilteredRoutes()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.AddRange(
            new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Metro },
            new Route { RouteId = "r-m2", ShortName = "M2", Type = TransitType.Metro },
            new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus }
        );
        await db.SaveChangesAsync();
        var repository = new RouteRepository(db);

        // Act
        var result = await repository.GetByTypeAsync(TransitType.Metro);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(TransitType.Metro, r.Type));
    }

    [Fact]
    public async Task GetByTypeAsync_WhenNoMatchingRoutes_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus });
        await db.SaveChangesAsync();
        var repository = new RouteRepository(db);

        // Act
        var result = await repository.GetByTypeAsync(TransitType.Trolley);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByShortNameAsync_WhenRouteExists_ReturnsRoute()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus });
        await db.SaveChangesAsync();
        var repository = new RouteRepository(db);

        // Act
        var result = await repository.GetByShortNameAsync("204");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("r-204", result!.RouteId);
    }

    [Fact]
    public async Task GetByShortNameAsync_WhenRouteDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new RouteRepository(db);

        // Act
        var result = await repository.GetByShortNameAsync("999");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenAllIdsMatch_ReturnsMatchingRoutes()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.AddRange(
            new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Metro },
            new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus },
            new Route { RouteId = "r-285", ShortName = "285", Type = TransitType.Bus }
        );
        await db.SaveChangesAsync();
        var repository = new RouteRepository(db);

        // Act
        var result = await repository.GetByIdsAsync(new[] { "r-1", "r-204" });

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.RouteId == "r-1");
        Assert.Contains(result, r => r.RouteId == "r-204");
    }

    [Fact]
    public async Task GetByIdsAsync_WhenEmptyList_ReturnsEmptyArray()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Metro });
        await db.SaveChangesAsync();
        var repository = new RouteRepository(db);

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
        db.Routes.AddRange(
            new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Metro },
            new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus }
        );
        await db.SaveChangesAsync();
        var repository = new RouteRepository(db);

        // Act
        var result = await repository.GetByIdsAsync(new[] { "r-1", "r-999" });

        // Assert
        Assert.Single(result);
        Assert.Equal("r-1", result[0].RouteId);
    }

    [Fact]
    public async Task AddAsync_AddsRoute_AndReturnsIt()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new RouteRepository(db);
        var route = new Route { RouteId = "r-new", ShortName = "NEW", Type = TransitType.Bus };

        // Act
        var result = await repository.AddAsync(route);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("r-new", result.RouteId);

        // Verify persisted
        var persisted = await db.Routes.FindAsync("r-new");
        Assert.NotNull(persisted);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingRoute()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus, LongName = "Old Name" });
        await db.SaveChangesAsync();
        var repository = new RouteRepository(db);

        var route = await db.Routes.FindAsync("r-204");
        route!.LongName = "Updated Name";

        // Act
        await repository.UpdateAsync(route);

        // Assert
        var updated = await db.Routes.FindAsync("r-204");
        Assert.Equal("Updated Name", updated!.LongName);
    }

    [Fact]
    public async Task DeleteAsync_DeletesRoute()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus });
        await db.SaveChangesAsync();
        var repository = new RouteRepository(db);
        var route = await db.Routes.FindAsync("r-204");

        // Act
        await repository.DeleteAsync(route!);

        // Assert
        var deleted = await db.Routes.FindAsync("r-204");
        Assert.Null(deleted);
    }
}
