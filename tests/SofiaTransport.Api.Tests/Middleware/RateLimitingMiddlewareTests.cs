using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SofiaTransport.Api.Middleware;

namespace SofiaTransport.Api.Tests.Middleware;

[Collection("RateLimiting")]
public class RateLimitingMiddlewareTests
{
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

    private static IConfiguration CreateConfig() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["RateLimit:MaxRequests"] = "100",
            ["RateLimit:WindowSeconds"] = "60"
        }).Build();

    [Fact]
    public async Task InvokeAsync_UnderLimit_PassesThrough()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.10");

        var nextCalled = false;
        Task next(HttpContext ctx) { nextCalled = true; return Task.CompletedTask; }

        var mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();
        var middleware = new RateLimitingMiddleware(next, mockLogger.Object, CreateConfig());

        for (var i = 0; i < 50; i++)
            await middleware.InvokeAsync(context);

        Assert.True(nextCalled, "Next delegate should be called when under limit");
    }

    [Fact]
    public async Task InvokeAsync_OverLimit_Returns429()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");

        var nextCalled = false;
        Task next(HttpContext ctx) { nextCalled = true; return Task.CompletedTask; }

        var mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();
        var middleware = new RateLimitingMiddleware(next, mockLogger.Object, CreateConfig());

        for (var i = 0; i < 100; i++)
            await middleware.InvokeAsync(context);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.True(context.Response.Headers.ContainsKey("Retry-After"));
    }

    [Fact]
    public async Task InvokeAsync_DifferentIPs_IndependentLimits()
    {
        var contextA = new DefaultHttpContext();
        contextA.Connection.RemoteIpAddress = IPAddress.Parse("10.10.10.1");
        var nextACalled = false;
        Task nextA(HttpContext ctx) { nextACalled = true; return Task.CompletedTask; }

        var contextB = new DefaultHttpContext();
        contextB.Connection.RemoteIpAddress = IPAddress.Parse("10.10.10.2");
        var nextBCalled = false;
        Task nextB(HttpContext ctx) { nextBCalled = true; return Task.CompletedTask; }

        var mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();
        var config = CreateConfig();
        var middlewareA = new RateLimitingMiddleware(nextA, mockLogger.Object, config);
        var middlewareB = new RateLimitingMiddleware(nextB, mockLogger.Object, config);

        for (var i = 0; i < 100; i++)
            await middlewareA.InvokeAsync(contextA);

        await middlewareA.InvokeAsync(contextA);
        await middlewareB.InvokeAsync(contextB);

        Assert.Equal(StatusCodes.Status429TooManyRequests, contextA.Response.StatusCode);
        Assert.True(nextACalled);
        Assert.True(nextBCalled);
    }

    [Fact]
    public async Task InvokeAsync_LogsWarning_WhenLimitExceeded()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("172.16.0.5");

        var mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();
        static Task next(HttpContext ctx) => Task.CompletedTask;
        var middleware = new RateLimitingMiddleware(next, mockLogger.Object, CreateConfig());

        for (var i = 0; i < 100; i++)
            await middleware.InvokeAsync(context);

        await middleware.InvokeAsync(context);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v!.ToString()!.Contains("Rate limit exceeded")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_IncludesRateLimitHeaders()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.99.99.1");
        Task next(HttpContext ctx) => Task.CompletedTask;

        var mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();
        var middleware = new RateLimitingMiddleware(next, mockLogger.Object, CreateConfig());

        await middleware.InvokeAsync(context);

        Assert.True(context.Response.Headers.ContainsKey("X-RateLimit-Limit"));
        Assert.True(context.Response.Headers.ContainsKey("X-RateLimit-Remaining"));
        Assert.True(context.Response.Headers.ContainsKey("X-RateLimit-Reset"));
    }
}
