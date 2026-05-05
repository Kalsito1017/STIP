using System.Security.Claims;
using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SofiaTransport.Api.Controllers;
using SofiaTransport.Application.Users;

namespace SofiaTransport.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly AuthController _controller;
    private readonly Mock<IMediator> _mockMediator;

    public AuthControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new AuthController(_mockMediator.Object);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new RegisterUserRequest("test@example.com", "P@ssw0rd!", "Test User");
        var authResponse = new AuthResponseDto(
            Guid.NewGuid(), "test@example.com", "Test User", "fake-jwt-token");

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("test@example.com", actual.Email);
        Assert.Equal("fake-jwt-token", actual.Token);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new LoginUserRequest("test@example.com", "P@ssw0rd!");
        var authResponse = new AuthResponseDto(
            Guid.NewGuid(), "test@example.com", "Test User", "fake-jwt-token");

        _mockMediator
            .Setup(m => m.Send(It.IsAny<LoginUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("test@example.com", actual.Email);
        Assert.Equal("fake-jwt-token", actual.Token);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginUserRequest("test@example.com", "wrong-password");

        _mockMediator
            .Setup(m => m.Send(It.IsAny<LoginUserQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Invalid email or password."));

        // Act & Assert — the controller does not catch this; ExceptionHandlingMiddleware converts to 400 at runtime.
        // At the unit-test level, we verify the validation exception propagates.
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _controller.Login(request));
    }

    [Fact]
    public async Task GetProfile_AuthenticatedUser_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserDto(userId, "test@example.com", "Test User", DateTime.UtcNow);

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        });
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        _mockMediator
            .Setup(m => m.Send(It.Is<GetUserProfileQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(userId, actual.Id);
        Assert.Equal("test@example.com", actual.Email);
    }

    [Fact]
    public async Task GetProfile_NoUserIdClaim_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        // Act
        var result = await _controller.GetProfile();

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetProfile_InvalidGuidFormat_ReturnsUnauthorized()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
        });
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await _controller.GetProfile();

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetProfile_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        });
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        _mockMediator
            .Setup(m => m.Send(It.Is<GetUserProfileQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteProfile_AuthenticatedUser_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        });
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        _mockMediator
            .Setup(m => m.Send(It.Is<DeleteUserCommand>(c => c.UserId == userId), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteProfile();

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteProfile_NoUserIdClaim_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        // Act
        var result = await _controller.DeleteProfile();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}
