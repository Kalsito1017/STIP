using Xunit;
using Moq;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Users;

namespace SofiaTransport.Application.Tests.Users;

public class DeleteUserHandlerTests
{
    [Fact]
    public async Task Handle_CallsDeleteAsync_WithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new DeleteUserHandler(mockUserRepo.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        mockUserRepo.Verify(r => r.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PassesCancellationToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        using var cts = new CancellationTokenSource();

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.DeleteAsync(userId, cts.Token))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var handler = new DeleteUserHandler(mockUserRepo.Object);

        // Act
        await handler.Handle(command, cts.Token);

        // Assert
        mockUserRepo.Verify(r => r.DeleteAsync(userId, cts.Token), Times.Once);
    }
}
