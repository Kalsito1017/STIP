using System.Net;
using System.Text.Json;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SofiaTransport.Api.Middleware;

namespace SofiaTransport.Api.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ExceptionIsCaught_Returns500WithJsonErrorBody()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        Task next(HttpContext ctx) => throw new InvalidOperationException("Test exception");

        var mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(next, mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();

        using var json = JsonDocument.Parse(body);
        Assert.True(json.RootElement.TryGetProperty("error", out var errorProp));
        Assert.Equal("An internal error occurred. Please try again later.", errorProp.GetString());
    }

    [Fact]
    public async Task InvokeAsync_NormalRequest_PassesThroughWithoutError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        Task next(HttpContext ctx)
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        }

        var mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(next, mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionIsLogged()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var exception = new InvalidOperationException("Log test exception");
        Task next(HttpContext ctx) => throw exception;

        var mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(next, mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
