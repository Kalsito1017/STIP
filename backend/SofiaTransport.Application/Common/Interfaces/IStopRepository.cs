using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IStopRepository : IRepository<Stop>
{
    Task<IReadOnlyList<Stop>> GetNearbyAsync(double lat, double lon, double radiusKm);
    Task<IReadOnlyList<Stop>> GetByIdsAsync(IReadOnlyList<string> stopIds);
}
