using System.Collections.Concurrent;

namespace SofiaTransport.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RateLimitEntry> _store = new();
    private const int MaxRequests = 100;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;

        var entry = _store.GetOrAdd(ip, _ => new RateLimitEntry { ResetTime = now.Add(Window), Count = 0 });

        lock (entry)
        {
            if (now > entry.ResetTime)
            {
                entry.ResetTime = now.Add(Window);
                entry.Count = 0;
            }

            entry.Count++;

            if (entry.Count > MaxRequests)
            {
                var retryAfter = (int)(entry.ResetTime - now).TotalSeconds;
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.RetryAfter = retryAfter.ToString();
                _logger.LogWarning("Rate limit exceeded for IP {IP}. Retry after {Seconds}s", ip, retryAfter);
                return;
            }
        }

        await _next(context);
    }

    private class RateLimitEntry
    {
        public DateTime ResetTime { get; set; }
        public int Count { get; set; }
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RateLimitingMiddleware>();
    }
}
