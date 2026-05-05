using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;
using SofiaTransport.Infrastructure.Persistence;
using SofiaTransport.Infrastructure.Persistence.Repositories;
using Xunit;
using Coordinates = SofiaTransport.Domain.ValueObjects.Coordinates;

namespace SofiaTransport.Infrastructure.Tests.Repositories;

public class VehicleRepositoryTests
{
    private static TransportDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TransportDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TransportDbContext(options);
    }

    private static Vehicle CreateVehicle(string vehicleId, string? routeId, DateTime recordedAt)
    {
        return new Vehicle
        {
            VehicleId = vehicleId,
            RouteId = routeId,
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 },
            RecordedAt = recordedAt
        };
    }

    [Fact]
    public async Task GetAllAsync_WhenVehiclesExist_ReturnsAllVehicles()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Vehicles.AddRange(
            CreateVehicle("v1", "r-1", DateTime.UtcNow),
            CreateVehicle("v2", "r-204", DateTime.UtcNow)
        );
        await db.SaveChangesAsync();
        var repository = new VehicleRepository(db);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, v => v.VehicleId == "v1");
        Assert.Contains(result, v => v.VehicleId == "v2");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoVehicles_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new VehicleRepository(db);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenVehicleExists_ReturnsVehicle()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Vehicles.Add(CreateVehicle("v1", "r-1", DateTime.UtcNow));
        await db.SaveChangesAsync();
        var repository = new VehicleRepository(db);

        // Act
        var result = await repository.GetByIdAsync("v1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("v1", result!.VehicleId);
        Assert.Equal("r-1", result.RouteId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenVehicleDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new VehicleRepository(db);

        // Act
        var result = await repository.GetByIdAsync("v-nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCountAsync_WhenVehiclesExist_ReturnsCorrectCount()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Vehicles.AddRange(
            CreateVehicle("v1", "r-1", DateTime.UtcNow),
            CreateVehicle("v2", "r-204", DateTime.UtcNow),
            CreateVehicle("v3", "r-285", DateTime.UtcNow)
        );
        await db.SaveChangesAsync();
        var repository = new VehicleRepository(db);

        // Act
        var result = await repository.GetCountAsync();

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task GetLiveAsync_ReturnsOnlyRecentVehicles()
    {
        // Arrange
        await using var db = CreateDbContext();
        var now = DateTime.UtcNow;
        db.Vehicles.AddRange(
            CreateVehicle("v1", "r-1", now),
            CreateVehicle("v2", "r-204", now.AddMinutes(-1)),
            CreateVehicle("v3", "r-285", now.AddMinutes(-5))
        );
        await db.SaveChangesAsync();
        var repository = new VehicleRepository(db);

        // Act
        var result = await repository.GetLiveAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, v => v.VehicleId == "v1");
        Assert.Contains(result, v => v.VehicleId == "v2");
    }

    [Fact]
    public async Task GetLiveAsync_WhenNoRecentVehicles_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Vehicles.Add(CreateVehicle("v1", "r-1", DateTime.UtcNow.AddMinutes(-10)));
        await db.SaveChangesAsync();
        var repository = new VehicleRepository(db);

        // Act
        var result = await repository.GetLiveAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByRouteAsync_ReturnsFilteredVehicles()
    {
        // Arrange
        await using var db = CreateDbContext();
        var now = DateTime.UtcNow;
        db.Vehicles.AddRange(
            CreateVehicle("v1", "r-1", now),
            CreateVehicle("v2", "r-204", now),
            CreateVehicle("v3", "r-1", now.AddMinutes(-1))
        );
        await db.SaveChangesAsync();
        var repository = new VehicleRepository(db);

        // Act
        var result = await repository.GetByRouteAsync("r-1");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, v => Assert.Equal("r-1", v.RouteId));
    }

    [Fact]
    public async Task GetByRouteAsync_WhenNoMatchingRoute_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Vehicles.Add(CreateVehicle("v1", "r-1", DateTime.UtcNow));
        await db.SaveChangesAsync();
        var repository = new VehicleRepository(db);

        // Act
        var result = await repository.GetByRouteAsync("r-999");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByRouteAsync_ExcludesStaleVehicles()
    {
        // Arrange
        await using var db = CreateDbContext();
        var now = DateTime.UtcNow;
        db.Vehicles.AddRange(
            CreateVehicle("v1", "r-1", now),
            CreateVehicle("v2", "r-1", now.AddMinutes(-10))
        );
        await db.SaveChangesAsync();
        var repository = new VehicleRepository(db);

        // Act
        var result = await repository.GetByRouteAsync("r-1");

        // Assert
        Assert.Single(result);
        Assert.Equal("v1", result[0].VehicleId);
    }

    [Fact]
    public async Task AddAsync_AddsVehicle_AndReturnsIt()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new VehicleRepository(db);
        var vehicle = CreateVehicle("v-new", "r-1", DateTime.UtcNow);

        // Act
        var result = await repository.AddAsync(vehicle);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("v-new", result.VehicleId);

        // Verify persisted
        var persisted = await db.Vehicles.FindAsync("v-new");
        Assert.NotNull(persisted);
    }
}
