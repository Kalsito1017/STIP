using Microsoft.EntityFrameworkCore;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Infrastructure.Persistence;
using SofiaTransport.Infrastructure.Persistence.Repositories;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.Repositories;

public class ReliabilityScoreRepositoryTests
{
    private static TransportDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TransportDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TransportDbContext(options);
    }

    private static ReliabilityScore CreateScore(string routeId, DateTime scoreDate, double score, double onTimePct, double avgDelaySeconds)
    {
        return new ReliabilityScore
        {
            RouteId = routeId,
            ScoreDate = scoreDate,
            Score = score,
            OnTimePct = onTimePct,
            AvgDelaySeconds = avgDelaySeconds,
            PeakScore = score * 0.8,
            SampleCount = 100
        };
    }

    [Fact]
    public async Task GetAllAsync_WhenScoresExist_ReturnsAllScores()
    {
        // Arrange
        await using var db = CreateDbContext();
        var date = new DateTime(2025, 5, 5, 0, 0, 0, DateTimeKind.Utc);
        db.ReliabilityScores.AddRange(
            CreateScore("r-1", date, 85.0, 0.9, 60),
            CreateScore("r-204", date, 75.0, 0.8, 120)
        );
        await db.SaveChangesAsync();
        var repository = new ReliabilityScoreRepository(db);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoScores_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new ReliabilityScoreRepository(db);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByRouteAndDateAsync_WhenScoreExists_ReturnsScore()
    {
        // Arrange
        await using var db = CreateDbContext();
        var date = new DateTime(2025, 5, 5, 12, 30, 0, DateTimeKind.Utc);
        db.ReliabilityScores.Add(CreateScore("r-1", date.Date, 85.0, 0.9, 60));
        await db.SaveChangesAsync();
        var repository = new ReliabilityScoreRepository(db);

        // Act
        var result = await repository.GetByRouteAndDateAsync("r-1", date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("r-1", result!.RouteId);
        Assert.Equal(date.Date, result.ScoreDate);
    }

    [Fact]
    public async Task GetByRouteAndDateAsync_WhenScoreDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new ReliabilityScoreRepository(db);

        // Act
        var result = await repository.GetByRouteAndDateAsync("r-1", DateTime.UtcNow);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByRouteAsync_WithoutDateFilter_ReturnsAllScoresForRoute()
    {
        // Arrange
        await using var db = CreateDbContext();
        var date = new DateTime(2025, 5, 5, 0, 0, 0, DateTimeKind.Utc);
        db.ReliabilityScores.AddRange(
            CreateScore("r-1", date, 85.0, 0.9, 60),
            CreateScore("r-1", date.AddDays(1), 87.0, 0.92, 50),
            CreateScore("r-204", date, 75.0, 0.8, 120)
        );
        await db.SaveChangesAsync();
        var repository = new ReliabilityScoreRepository(db);

        // Act
        var result = await repository.GetByRouteAsync("r-1");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.Equal("r-1", s.RouteId));
    }

    [Fact]
    public async Task GetByRouteAsync_WithDateFilter_ReturnsFilteredScores()
    {
        // Arrange
        await using var db = CreateDbContext();
        var date = new DateTime(2025, 5, 5, 0, 0, 0, DateTimeKind.Utc);
        db.ReliabilityScores.AddRange(
            CreateScore("r-1", date, 85.0, 0.9, 60),
            CreateScore("r-1", date.AddDays(1), 87.0, 0.92, 50),
            CreateScore("r-1", date.AddDays(2), 80.0, 0.85, 70)
        );
        await db.SaveChangesAsync();
        var repository = new ReliabilityScoreRepository(db);

        // Act
        var result = await repository.GetByRouteAsync("r-1", date, date.AddDays(1));

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByRouteAsync_WhenNoMatchingRoute_ReturnsEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new ReliabilityScoreRepository(db);

        // Act
        var result = await repository.GetByRouteAsync("r-999");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLatestByRouteAsync_ReturnsMostRecentScore()
    {
        // Arrange
        await using var db = CreateDbContext();
        var date = new DateTime(2025, 5, 5, 0, 0, 0, DateTimeKind.Utc);
        db.ReliabilityScores.AddRange(
            CreateScore("r-1", date, 85.0, 0.9, 60),
            CreateScore("r-1", date.AddDays(1), 87.0, 0.92, 50)
        );
        await db.SaveChangesAsync();
        var repository = new ReliabilityScoreRepository(db);

        // Act
        var result = await repository.GetLatestByRouteAsync("r-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(date.AddDays(1), result!.ScoreDate);
    }

    [Fact]
    public async Task GetLatestByRouteAsync_WhenNoScoreExists_ReturnsNull()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new ReliabilityScoreRepository(db);

        // Act
        var result = await repository.GetLatestByRouteAsync("r-1");

        // Assert
        Assert.Null(result);
    }

    // InMemory EF Core cannot translate GroupBy+OrderByDescending+First pattern used by GetRankingAsync
    [Fact(Skip = "GetRankingAsync uses GroupBy with subquery that InMemory provider cannot translate")]
    public async Task GetRankingAsync_WithBestTrue_ReturnsTopScores()
    {
        // Arrange
        await using var db = CreateDbContext();
        var date = new DateTime(2025, 5, 5, 0, 0, 0, DateTimeKind.Utc);
        db.ReliabilityScores.AddRange(
            CreateScore("r-1", date, 50.0, 0.5, 300),
            CreateScore("r-204", date, 90.0, 0.95, 30),
            CreateScore("r-285", date, 70.0, 0.75, 100)
        );
        await db.SaveChangesAsync();
        var repository = new ReliabilityScoreRepository(db);

        // Act
        var result = await repository.GetRankingAsync(top: 2, best: true);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result[0].Score >= result[1].Score);
    }

    // InMemory EF Core cannot translate GroupBy+OrderByDescending+First pattern used by GetRankingAsync
    [Fact(Skip = "GetRankingAsync uses GroupBy with subquery that InMemory provider cannot translate")]
    public async Task GetRankingAsync_WithBestFalse_ReturnsWorstScores()
    {
        // Arrange
        await using var db = CreateDbContext();
        var date = new DateTime(2025, 5, 5, 0, 0, 0, DateTimeKind.Utc);
        db.ReliabilityScores.AddRange(
            CreateScore("r-1", date, 50.0, 0.5, 300),
            CreateScore("r-204", date, 90.0, 0.95, 30),
            CreateScore("r-285", date, 70.0, 0.75, 100)
        );
        await db.SaveChangesAsync();
        var repository = new ReliabilityScoreRepository(db);

        // Act
        var result = await repository.GetRankingAsync(top: 2, best: false);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result[0].Score <= result[1].Score);
    }

    [Fact]
    public async Task AddAsync_AddsScore_AndReturnsIt()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new ReliabilityScoreRepository(db);
        var score = CreateScore("r-new", DateTime.UtcNow.Date, 88.0, 0.93, 45);

        // Act
        var result = await repository.AddAsync(score);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("r-new", result.RouteId);

        // Verify persisted
        var persisted = await db.ReliabilityScores.FindAsync("r-new", DateTime.UtcNow.Date);
        Assert.NotNull(persisted);
    }
}
