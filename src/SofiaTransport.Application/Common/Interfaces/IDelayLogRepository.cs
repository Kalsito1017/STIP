using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IDelayLogRepository : IRepository<DelayLog>
{
    Task<IReadOnlyList<DelayLog>> GetByRouteAsync(string routeId, DateTime from, DateTime to);
    Task<IReadOnlyList<DelayLog>> GetByStopAsync(string stopId, DateTime from, DateTime to);
    Task<IReadOnlyList<DelayLog>> GetForHeatmapAsync(DateTime from, DateTime to);
    Task<IReadOnlyList<DelayLog>> GetByDateAsync(DateTime date);
}
