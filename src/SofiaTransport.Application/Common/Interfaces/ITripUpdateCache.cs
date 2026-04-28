using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface ITripUpdateCache
{
    Task<IReadOnlyList<TripUpdate>> GetAllAsync();
    Task<IReadOnlyList<TripUpdate>> GetByRouteAsync(string routeId);
    Task SetAsync(TripUpdate tripUpdate);
    Task RemoveAsync(string tripId);
}