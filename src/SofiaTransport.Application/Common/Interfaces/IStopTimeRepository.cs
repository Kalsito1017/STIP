using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IStopTimeRepository
{
    Task<IReadOnlyList<StopTime>> GetUpcomingByStopAsync(string stopId, TimeSpan fromTime, int limit);
    Task<IReadOnlyList<StopTime>> GetByTripAsync(string tripId);
    Task<IReadOnlyList<StopTime>> GetByStopAndRouteAsync(string stopId, string routeId);
}
