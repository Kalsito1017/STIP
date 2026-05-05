using System.Collections.Concurrent;

namespace SofiaTransport.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _maxRequests;
    private readonly TimeSpan _window;
    private static readonly ConcurrentDictionary<string, RateLimitEntry> _store = new();

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _maxRequests = configuration.GetValue("RateLimit:MaxRequests", 100);
        _window = TimeSpan.FromSeconds(configuration.GetValue("RateLimit:WindowSeconds", 60));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;

        var entry = _store.GetOrAdd(ip, _ => new RateLimitEntry { ResetTime = now.Add(_window), Count = 0 });

        int count;
        DateTime resetTime;
        lock (entry)
        {
            if (now > entry.ResetTime)
            {
                entry.ResetTime = now.Add(_window);
                entry.Count = 0;
            }

            entry.Count++;
            entry.LastAccess = now;
            count = entry.Count;
            resetTime = entry.ResetTime;
        }

        var remaining = Math.Max(0, _maxRequests - count);
        var retryAfter = (int)(resetTime - now).TotalSeconds;

        context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(resetTime).ToUnixTimeSeconds().ToString();

        if (count > _maxRequests)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = retryAfter.ToString();
            _logger.LogWarning("Rate limit exceeded for IP {IP}. Retry after {Seconds}s", ip, retryAfter);
            return;
        }

        if (count % 200 == 0)
            PruneExpiredEntries(now);

        await _next(context);
    }

    private static void PruneExpiredEntries(DateTime now)
    {
        var expired = _store.Where(kvp => now > kvp.Value.LastAccess.AddMinutes(5))
            .Select(kvp => kvp.Key).ToList();
        foreach (var key in expired)
            _store.TryRemove(key, out _);
    }

    private class RateLimitEntry
    {
        public DateTime ResetTime { get; set; }
        public DateTime LastAccess { get; set; }
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
