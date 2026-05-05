using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Users;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Tests.Users;

public class RegisterUserValidatorTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;

    public RegisterUserValidatorTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
    }

    [Fact]
    public async Task Validate_ValidCommand_Passes()
    {
        // Arrange
        var validator = new RegisterUserValidator(_mockUserRepo.Object);
        var command = new RegisterUserCommand("test@example.com", "password123", "Test User");

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_EmptyEmail_Fails()
    {
        // Arrange
        var validator = new RegisterUserValidator(_mockUserRepo.Object);
        var command = new RegisterUserCommand("", "password123", "Test User");

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email is required.");
    }

    [Fact]
    public async Task Validate_InvalidEmailFormat_Fails()
    {
        // Arrange
        var validator = new RegisterUserValidator(_mockUserRepo.Object);
        var command = new RegisterUserCommand("not-an-email", "password123", "Test User");

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "A valid email address is required.");
    }

    [Fact]
    public async Task Validate_ShortPassword_Fails()
    {
        // Arrange
        var validator = new RegisterUserValidator(_mockUserRepo.Object);
        var command = new RegisterUserCommand("test@example.com", "12345", "Test User");

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password must be at least 6 characters.");
    }

    [Fact]
    public async Task Validate_EmptyFullName_Fails()
    {
        // Arrange
        var validator = new RegisterUserValidator(_mockUserRepo.Object);
        var command = new RegisterUserCommand("test@example.com", "password123", "");

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Full name is required.");
    }
}
