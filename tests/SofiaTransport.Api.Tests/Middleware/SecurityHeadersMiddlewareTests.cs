using Xunit;
using Microsoft.AspNetCore.Http;
using SofiaTransport.Api.Middleware;

namespace SofiaTransport.Api.Tests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_SetsAllFiveSecurityHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedHeaders = new Dictionary<string, string>
        {
            { "X-Content-Type-Options", "nosniff" },
            { "X-Frame-Options", "DENY" },
            { "Content-Security-Policy", "default-src 'self'" },
            { "Referrer-Policy", "strict-origin-when-cross-origin" },
            { "Permissions-Policy", "geolocation=(self)" }
        };

        static Task next(HttpContext ctx) => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        foreach (var (name, value) in expectedHeaders)
        {
            Assert.True(context.Response.Headers.ContainsKey(name), $"Missing header: {name}");
            Assert.Equal(value, context.Response.Headers[name]);
        }
    }

    [Fact]
    public async Task InvokeAsync_CallsNextDelegate()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }
        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled, "Next delegate should be called");
    }
}
