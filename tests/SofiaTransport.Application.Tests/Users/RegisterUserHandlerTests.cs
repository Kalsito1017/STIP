using FluentValidation;
using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Users;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Tests.Users;

public class RegisterUserHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsAuthResponseDto()
    {
        // Arrange
        var command = new RegisterUserCommand(" Test@Example.com ", "securePass123", "Test User");
        var normalizedEmail = "test@example.com";

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByEmailAsync(normalizedEmail)).ReturnsAsync((User?)null);

        User capturedUser = null!;
        mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync((User u, CancellationToken _) => u);

        var mockTokenService = new Mock<ITokenService>();
        mockTokenService.Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns("fake-jwt-token");

        var handler = new RegisterUserHandler(mockUserRepo.Object, mockTokenService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(normalizedEmail, result.Email);
        Assert.Equal("Test User", result.FullName);
        Assert.Equal("fake-jwt-token", result.Token);
        Assert.NotEqual(Guid.Empty, result.UserId);

        Assert.NotNull(capturedUser);
        Assert.Equal(normalizedEmail, capturedUser.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify("securePass123", capturedUser.PasswordHash));
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsValidationException()
    {
        // Arrange
        var command = new RegisterUserCommand("existing@example.com", "securePass123", "Test User");
        var normalizedEmail = "existing@example.com";

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            FullName = "Existing User"
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByEmailAsync(normalizedEmail)).ReturnsAsync(existingUser);

        var mockTokenService = new Mock<ITokenService>();

        var handler = new RegisterUserHandler(mockUserRepo.Object, mockTokenService.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
        mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailIsNormalized_TrimsAndLowercases()
    {
        // Arrange
        var command = new RegisterUserCommand("  TEST@EXAMPLE.COM  ", "securePass123", "Test User");
        var normalizedEmail = "test@example.com";

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByEmailAsync(normalizedEmail)).ReturnsAsync((User?)null);

        User capturedUser = null!;
        mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync((User u, CancellationToken _) => u);

        var mockTokenService = new Mock<ITokenService>();
        mockTokenService.Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns("fake-jwt-token");

        var handler = new RegisterUserHandler(mockUserRepo.Object, mockTokenService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(normalizedEmail, result.Email);
        Assert.Equal(normalizedEmail, capturedUser.Email);
    }

    [Fact]
    public async Task Handle_PasswordIsHashed_WithBCrypt()
    {
        // Arrange
        var command = new RegisterUserCommand("user@example.com", "mySecret123", "Test User");
        var normalizedEmail = "user@example.com";

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByEmailAsync(normalizedEmail)).ReturnsAsync((User?)null);

        User capturedUser = null!;
        mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync((User u, CancellationToken _) => u);

        var mockTokenService = new Mock<ITokenService>();
        mockTokenService.Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns("fake-jwt-token");

        var handler = new RegisterUserHandler(mockUserRepo.Object, mockTokenService.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedUser);
        // The hash should not be the plaintext password
        Assert.NotEqual("mySecret123", capturedUser.PasswordHash);
        // BCrypt.Verify should confirm the hash matches
        Assert.True(BCrypt.Net.BCrypt.Verify("mySecret123", capturedUser.PasswordHash));
    }
}
