using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Users;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Tests.Users;

public class GetUserProfileHandlerTests
{
    [Fact]
    public async Task Handle_UserFound_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserProfileQuery(userId);

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FullName = "Test User",
            CreatedAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var handler = new GetUserProfileHandler(mockUserRepo.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.FullName);
        Assert.Equal(user.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserProfileQuery(userId);

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var handler = new GetUserProfileHandler(mockUserRepo.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
