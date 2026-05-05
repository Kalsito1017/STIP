using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Users;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Tests.Users;

public class LoginUserHandlerTests
{
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponseDto()
    {
        // Arrange
        var query = new LoginUserQuery(" Test@Example.com ", "securePass123");
        var normalizedEmail = "test@example.com";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("securePass123", 12),
            FullName = "Test User"
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByEmailAsync(normalizedEmail)).ReturnsAsync(user);

        var mockTokenService = new Mock<ITokenService>();
        mockTokenService.Setup(t => t.GenerateToken(user)).Returns("fake-jwt-token");

        var handler = new LoginUserHandler(mockUserRepo.Object, mockTokenService.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(normalizedEmail, result.Email);
        Assert.Equal("Test User", result.FullName);
        Assert.Equal("fake-jwt-token", result.Token);
        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    public async Task Handle_UsesNormalizedEmail_ForLookup()
    {
        // Arrange
        var query = new LoginUserQuery("  UPPER@EXAMPLE.COM  ", "securePass123");
        var normalizedEmail = "upper@example.com";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("securePass123", 12),
            FullName = "Upper User"
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByEmailAsync(normalizedEmail)).ReturnsAsync(user);
        // Verify the call uses normalized email, not raw
        mockUserRepo.Setup(r => r.GetByEmailAsync(It.Is<string>(e => e == normalizedEmail)))
            .ReturnsAsync(user)
            .Verifiable();

        var mockTokenService = new Mock<ITokenService>();
        mockTokenService.Setup(t => t.GenerateToken(user)).Returns("fake-jwt-token");

        var handler = new LoginUserHandler(mockUserRepo.Object, mockTokenService.Object);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        mockUserRepo.Verify(r => r.GetByEmailAsync(normalizedEmail), Times.Once);
    }
}
