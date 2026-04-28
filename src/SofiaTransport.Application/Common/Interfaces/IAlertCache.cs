using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IAlertCache
{
    Task<IReadOnlyList<ServiceAlert>> GetAllAsync();
    Task<IReadOnlyList<ServiceAlert>> GetByRouteAsync(string routeId);
    Task SetAsync(ServiceAlert alert);
    Task RemoveAsync(string alertId);
}