using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Moq;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Infrastructure.Auth;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.Auth;

public class TokenServiceTests
{
    private static IConfiguration CreateConfiguration(string? secret = null, string? issuer = null, string? audience = null)
    {
        var mockConfig = new Mock<IConfiguration>();
        if (secret is not null)
            mockConfig.Setup(c => c["Jwt:Secret"]).Returns(secret);
        if (issuer is not null)
            mockConfig.Setup(c => c["Jwt:Issuer"]).Returns(issuer);
        if (audience is not null)
            mockConfig.Setup(c => c["Jwt:Audience"]).Returns(audience);
        return mockConfig.Object;
    }

    [Fact]
    public void GenerateToken_ReturnsNonNullString()
    {
        // Arrange
        var config = CreateConfiguration(
            secret: "test-secret-key-thats-at-least-32-chars!!",
            issuer: "STIP",
            audience: "STIP");
        var service = new TokenService(config);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };

        // Act
        var token = service.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_WhenSecretMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = CreateConfiguration(secret: null);
        var service = new TokenService(config);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => service.GenerateToken(user));
        Assert.Equal("Jwt:Secret configuration is required", ex.Message);
    }

    [Fact]
    public void GenerateToken_TokenContainsCorrectClaims()
    {
        // Arrange
        var config = CreateConfiguration(
            secret: "test-secret-key-thats-at-least-32-chars!!",
            issuer: "STIP",
            audience: "STIP");
        var service = new TokenService(config);
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };

        // Act
        var token = service.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(userId.ToString(), jwt.Subject);
        Assert.Equal("user@example.com", jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value);
        Assert.Equal("Test User", jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value);
        Assert.NotNull(jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value);
    }

    [Fact]
    public void GenerateToken_UsesConfiguredIssuer()
    {
        // Arrange
        var config = CreateConfiguration(
            secret: "test-secret-key-thats-at-least-32-chars!!",
            issuer: "CustomIssuer",
            audience: "CustomAudience");
        var service = new TokenService(config);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };

        // Act
        var token = service.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        Assert.Equal("CustomIssuer", jwt.Issuer);
        Assert.Contains("CustomAudience", jwt.Audiences);
    }
}
