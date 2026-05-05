using Microsoft.EntityFrameworkCore;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Infrastructure.Persistence;
using SofiaTransport.Infrastructure.Persistence.Repositories;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.Repositories;

public class UserRepositoryTests
{
    private static TransportDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TransportDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TransportDbContext(options);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash",
            FullName = "Test User"
        });
        await db.SaveChangesAsync();
        var repository = new UserRepository(db);

        // Act
        var result = await repository.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result!.Id);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new UserRepository(db);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            FullName = "Test User"
        });
        await db.SaveChangesAsync();
        var repository = new UserRepository(db);

        // Act
        var result = await repository.GetByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result!.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_NormalizesEmailToLowercase()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            FullName = "Test User"
        });
        await db.SaveChangesAsync();
        var repository = new UserRepository(db);

        // Act
        var result = await repository.GetByEmailAsync("TEST@EXAMPLE.COM");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result!.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_NormalizesEmailWithWhitespace()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            FullName = "Test User"
        });
        await db.SaveChangesAsync();
        var repository = new UserRepository(db);

        // Act
        var result = await repository.GetByEmailAsync("  Test@Example.COM  ");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result!.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new UserRepository(db);

        // Act
        var result = await repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_AddsUser_AndReturnsIt()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new UserRepository(db);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "new@example.com",
            PasswordHash = "hash",
            FullName = "New User"
        };

        // Act
        var result = await repository.AddAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("new@example.com", result.Email);

        // Verify persisted
        var persisted = await db.Users.FindAsync(user.Id);
        Assert.NotNull(persisted);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_DeletesUser()
    {
        // Arrange
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = userId,
            Email = "delete@example.com",
            PasswordHash = "hash",
            FullName = "Delete Me"
        });
        await db.SaveChangesAsync();
        var repository = new UserRepository(db);

        // Act
        await repository.DeleteAsync(userId);

        // Assert
        var deleted = await db.Users.FindAsync(userId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserDoesNotExist_DoesNotThrow()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new UserRepository(db);

        // Act & Assert - should not throw
        await repository.DeleteAsync(Guid.NewGuid());
    }
}
