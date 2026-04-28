using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IShapeRepository
{
    Task<IReadOnlyList<Shape>> GetByRouteIdAsync(string routeId);
    Task<IReadOnlyList<Shape>> GetAllGroupedByRouteAsync();
}
