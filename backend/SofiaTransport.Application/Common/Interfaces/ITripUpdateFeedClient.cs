using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface ITripUpdateFeedClient
{
    Task<IReadOnlyList<TripUpdate>> FetchTripUpdatesAsync(CancellationToken ct);
}