using Xunit;

namespace SofiaTransport.Api.Tests.Middleware;

/// <summary>
/// Defines a test collection that prevents parallel execution of RateLimitingMiddleware tests,
/// since the middleware uses a static <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
[CollectionDefinition("RateLimiting")]
public class RateLimitingCollection : ICollectionFixture<object>
{
    // No shared fixture needed; the collection just disables parallelization.
}
