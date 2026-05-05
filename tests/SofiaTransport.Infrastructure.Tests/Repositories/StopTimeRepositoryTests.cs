using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;
using SofiaTransport.Domain.ValueObjects;
using SofiaTransport.Infrastructure.Persistence;
using SofiaTransport.Infrastructure.Persistence.Repositories;
using Xunit;
using Coordinates = SofiaTransport.Domain.ValueObjects.Coordinates;

namespace SofiaTransport.Infrastructure.Tests.Repositories;

public class StopTimeRepositoryTests
{
    private static TransportDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TransportDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TransportDbContext(options);
    }

    [Fact]
    public async Task GetUpcomingByStopAsync_ReturnsUpcomingStopTimesOrderedByArrival()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Bus });
        db.Stops.Add(new Stop
        {
            StopId = "s-001",
            StopName = "Test Stop",
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 }
        });
        var trip1 = new Trip { TripId = "t1", RouteId = "r-1", ServiceId = "wd", DirectionId = 0 };
        var trip2 = new Trip { TripId = "t2", RouteId = "r-1", ServiceId = "wd", DirectionId = 0 };
        db.Trips.AddRange(trip1, trip2);
        db.StopTimes.AddRange(
            new StopTime { TripId = "t1", StopId = "s-001", StopSequence = 1, ArrivalTime = TimeSpan.FromHours(9), DepartureTime = TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(1)) },
            new StopTime { TripId = "t2", StopId = "s-001", StopSequence = 1, ArrivalTime = TimeSpan.FromHours(8), DepartureTime = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(1)) },
            new StopTime { TripId = "t1", StopId = "s-002", StopSequence = 2, ArrivalTime = TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(5)) }
        );
        await db.SaveChangesAsync();
        var repository = new StopTimeRepository(db);

        // Act
        var result = await repository.GetUpcomingByStopAsync("s-001", TimeSpan.FromHours(7), 10);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result[0].ArrivalTime <= result[1].ArrivalTime);
    }

    [Fact]
    public async Task GetUpcomingByStopAsync_WithTimeFilter_ExcludesPastTimes()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Bus });
        db.Stops.Add(new Stop
        {
            StopId = "s-001",
            StopName = "Test Stop",
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 }
        });
        var trip = new Trip { TripId = "t1", RouteId = "r-1", ServiceId = "wd", DirectionId = 0 };
        db.Trips.Add(trip);
        db.StopTimes.AddRange(
            new StopTime { TripId = "t1", StopId = "s-001", StopSequence = 1, ArrivalTime = TimeSpan.FromHours(8), DepartureTime = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(1)) },
            new StopTime { TripId = "t1", StopId = "s-001", StopSequence = 2, ArrivalTime = TimeSpan.FromHours(12), DepartureTime = TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(1)) }
        );
        await db.SaveChangesAsync();
        var repository = new StopTimeRepository(db);

        // Act
        var result = await repository.GetUpcomingByStopAsync("s-001", TimeSpan.FromHours(10), 10);

        // Assert
        Assert.Single(result);
        Assert.Equal(TimeSpan.FromHours(12), result[0].ArrivalTime);
    }

    [Fact]
    public async Task GetUpcomingByStopAsync_WithLimit_ReturnsLimitedResults()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Bus });
        db.Stops.Add(new Stop
        {
            StopId = "s-001",
            StopName = "Test Stop",
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 }
        });
        db.Trips.Add(new Trip { TripId = "t1", RouteId = "r-1", ServiceId = "wd", DirectionId = 0 });
        db.Trips.Add(new Trip { TripId = "t2", RouteId = "r-1", ServiceId = "wd", DirectionId = 0 });
        db.Trips.Add(new Trip { TripId = "t3", RouteId = "r-1", ServiceId = "wd", DirectionId = 0 });
        db.StopTimes.AddRange(
            new StopTime { TripId = "t1", StopId = "s-001", StopSequence = 1, ArrivalTime = TimeSpan.FromHours(9) },
            new StopTime { TripId = "t2", StopId = "s-001", StopSequence = 1, ArrivalTime = TimeSpan.FromHours(10) },
            new StopTime { TripId = "t3", StopId = "s-001", StopSequence = 1, ArrivalTime = TimeSpan.FromHours(11) }
        );
        await db.SaveChangesAsync();
        var repository = new StopTimeRepository(db);

        // Act
        var result = await repository.GetUpcomingByStopAsync("s-001", TimeSpan.FromHours(8), 2);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetUpcomingByStopAsync_WhenNoMatch_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new StopTimeRepository(db);

        // Act
        var result = await repository.GetUpcomingByStopAsync("s-001", TimeSpan.FromHours(8), 10);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByTripAsync_ReturnsStopTimesOrderedBySequence()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Bus });
        db.Stops.Add(new Stop
        {
            StopId = "s-001",
            StopName = "Stop 1",
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 }
        });
        db.Stops.Add(new Stop
        {
            StopId = "s-002",
            StopName = "Stop 2",
            Location = new Coordinates(42.70, 23.34),
            Geometry = new Point(23.34, 42.70) { SRID = 4326 }
        });
        db.Trips.Add(new Trip { TripId = "t1", RouteId = "r-1", ServiceId = "wd", DirectionId = 0 });
        db.StopTimes.AddRange(
            new StopTime { TripId = "t1", StopId = "s-002", StopSequence = 2, ArrivalTime = TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(5)) },
            new StopTime { TripId = "t1", StopId = "s-001", StopSequence = 1, ArrivalTime = TimeSpan.FromHours(9) }
        );
        await db.SaveChangesAsync();
        var repository = new StopTimeRepository(db);

        // Act
        var result = await repository.GetByTripAsync("t1");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].StopSequence);
        Assert.Equal(2, result[1].StopSequence);
    }

    [Fact]
    public async Task GetByTripAsync_WhenTripDoesNotExist_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new StopTimeRepository(db);

        // Act
        var result = await repository.GetByTripAsync("t-nonexistent");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByStopAndRouteAsync_ReturnsMatchingStopTimes()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.AddRange(
            new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Bus },
            new Route { RouteId = "r-204", ShortName = "204", Type = TransitType.Bus }
        );
        db.Stops.Add(new Stop
        {
            StopId = "s-001",
            StopName = "Test Stop",
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 }
        });
        db.Trips.AddRange(
            new Trip { TripId = "t1", RouteId = "r-1", ServiceId = "wd", DirectionId = 0 },
            new Trip { TripId = "t2", RouteId = "r-204", ServiceId = "wd", DirectionId = 0 },
            new Trip { TripId = "t3", RouteId = "r-1", ServiceId = "wd", DirectionId = 0 }
        );
        db.StopTimes.AddRange(
            new StopTime { TripId = "t1", StopId = "s-001", StopSequence = 1, ArrivalTime = TimeSpan.FromHours(9) },
            new StopTime { TripId = "t2", StopId = "s-001", StopSequence = 1, ArrivalTime = TimeSpan.FromHours(10) },
            new StopTime { TripId = "t3", StopId = "s-001", StopSequence = 1, ArrivalTime = TimeSpan.FromHours(11) }
        );
        await db.SaveChangesAsync();
        var repository = new StopTimeRepository(db);

        // Act
        var result = await repository.GetByStopAndRouteAsync("s-001", "r-1");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, st => Assert.Equal("s-001", st.StopId));
        Assert.All(result, st => Assert.Equal("r-1", st.Trip.RouteId));
    }

    [Fact]
    public async Task GetByStopAndRouteAsync_WhenNoMatch_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Routes.Add(new Route { RouteId = "r-1", ShortName = "1", Type = TransitType.Bus });
        db.Stops.Add(new Stop
        {
            StopId = "s-001",
            StopName = "Test Stop",
            Location = new Coordinates(42.69, 23.33),
            Geometry = new Point(23.33, 42.69) { SRID = 4326 }
        });
        db.Trips.Add(new Trip { TripId = "t1", RouteId = "r-1", ServiceId = "wd", DirectionId = 0 });
        db.StopTimes.Add(new StopTime { TripId = "t1", StopId = "s-001", StopSequence = 1, ArrivalTime = TimeSpan.FromHours(9) });
        await db.SaveChangesAsync();
        var repository = new StopTimeRepository(db);

        // Act
        var result = await repository.GetByStopAndRouteAsync("s-999", "r-1");

        // Assert
        Assert.Empty(result);
    }
}
