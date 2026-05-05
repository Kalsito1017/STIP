using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Users;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Tests.Users;

public class LoginUserValidatorTests
{
    [Fact]
    public async Task Validate_ValidQuery_Passes()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123", 12);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = passwordHash,
            FullName = "Test User"
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

        var validator = new LoginUserValidator(mockUserRepo.Object);
        var query = new LoginUserQuery("test@example.com", "password123");

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_EmptyEmail_Fails()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();
        var validator = new LoginUserValidator(mockUserRepo.Object);
        var query = new LoginUserQuery("", "password123");

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email is required.");
    }

    [Fact]
    public async Task Validate_EmptyPassword_Fails()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();
        var validator = new LoginUserValidator(mockUserRepo.Object);
        var query = new LoginUserQuery("test@example.com", "");

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password is required.");
    }

    [Fact]
    public async Task Validate_InvalidCredentials_MockReturnsNull_Fails()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByEmailAsync("nonexistent@example.com"))
            .ReturnsAsync((User?)null);

        var validator = new LoginUserValidator(mockUserRepo.Object);
        var query = new LoginUserQuery("nonexistent@example.com", "wrongpassword");

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Invalid email or password.");
    }
}
