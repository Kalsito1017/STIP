using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IReliabilityScoreRepository : IRepository<ReliabilityScore>
{
    Task<ReliabilityScore?> GetByRouteAndDateAsync(string routeId, DateTime date);
    Task<IReadOnlyList<ReliabilityScore>> GetByRouteAsync(string routeId);
    Task<IReadOnlyList<ReliabilityScore>> GetRankingAsync(int top = 10, bool best = true);
}
