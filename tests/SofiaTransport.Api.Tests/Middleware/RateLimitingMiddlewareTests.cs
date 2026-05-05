using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SofiaTransport.Api.Middleware;

namespace SofiaTransport.Api.Tests.Middleware;

/// <summary>
/// Tests for <see cref="RateLimitingMiddleware"/>.
/// Uses <see cref="RateLimitingCollection"/> to prevent parallel test interference
/// since the middleware relies on a static <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
[Collection("RateLimiting")]
public class RateLimitingMiddlewareTests
{
    /// <summary>
    /// Clears the static rate-limit store before each test so tests don't interfere.
    /// </summary>
    public RateLimitingMiddlewareTests()
    {
        ClearRateLimitStore();
    }

    private static void ClearRateLimitStore()
    {
        var storeField = typeof(RateLimitingMiddleware)
            .GetField("_store", BindingFlags.Static | BindingFlags.NonPublic);
        var store = storeField?.GetValue(null) as ConcurrentDictionary<string, object>;
        store?.Clear();
    }

    [Fact]
    public async Task InvokeAsync_UnderLimit_PassesThrough()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.10");

        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();
        var middleware = new RateLimitingMiddleware(next, mockLogger.Object);

        // Act — call 50 times, well under the 100 limit
        for (var i = 0; i < 50; i++)
        {
            await middleware.InvokeAsync(context);
        }

        // Assert
        Assert.True(nextCalled, "Next delegate should be called when under limit");
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_OverLimit_Returns429()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");

        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();
        var middleware = new RateLimitingMiddleware(next, mockLogger.Object);

        // Act — call 101 times, exceeding the 100 limit
        // The first 100 should pass through; the 101st returns 429
        for (var i = 0; i < 100; i++)
        {
            await middleware.InvokeAsync(context);
        }

        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled, "Next should have been called for the first 100 requests");
        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.True(context.Response.Headers.ContainsKey("Retry-After"));
    }

    [Fact]
    public async Task InvokeAsync_DifferentIPs_IndependentLimits()
    {
        // Arrange
        var contextA = new DefaultHttpContext();
        contextA.Connection.RemoteIpAddress = IPAddress.Parse("10.10.10.1");
        var nextACalled = false;
        Task nextA(HttpContext ctx)
        {
            nextACalled = true;
            return Task.CompletedTask;
        }

        var contextB = new DefaultHttpContext();
        contextB.Connection.RemoteIpAddress = IPAddress.Parse("10.10.10.2");
        var nextBCalled = false;
        Task nextB(HttpContext ctx)
        {
            nextBCalled = true;
            return Task.CompletedTask;
        }

        var mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();
        var middlewareA = new RateLimitingMiddleware(nextA, mockLogger.Object);
        var middlewareB = new RateLimitingMiddleware(nextB, mockLogger.Object);

        // Act — exhaust IP A's limit but leave IP B under it
        for (var i = 0; i < 100; i++)
        {
            await middlewareA.InvokeAsync(contextA);
        }

        await middlewareA.InvokeAsync(contextA); // 101st for A → 429
        await middlewareB.InvokeAsync(contextB); // 1st for B → should pass

        // Assert
        Assert.Equal(StatusCodes.Status429TooManyRequests, contextA.Response.StatusCode);
        Assert.True(nextACalled, "Next should have been called for A's first 100 requests");
        Assert.True(nextBCalled, "Next should have been called for B's separate limit");
    }

    [Fact]
    public async Task InvokeAsync_LogsWarning_WhenLimitExceeded()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("172.16.0.5");

        var mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();
        static Task next(HttpContext ctx) => Task.CompletedTask;
        var middleware = new RateLimitingMiddleware(next, mockLogger.Object);

        // Act — hit limit then exceed it
        for (var i = 0; i < 100; i++)
        {
            await middleware.InvokeAsync(context);
        }

        await middleware.InvokeAsync(context); // This one triggers the warning log

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v!.ToString()!.Contains("Rate limit exceeded")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
