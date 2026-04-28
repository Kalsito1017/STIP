using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IRouteRepository : IRepository<Route>
{
    Task<Route?> GetByShortNameAsync(string shortName);
    Task<IReadOnlyList<Route>> GetByTypeAsync(Domain.Enums.TransitType type);
}
