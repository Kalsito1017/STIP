using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<IReadOnlyList<Vehicle>> GetLiveAsync();
    Task<IReadOnlyList<Vehicle>> GetByRouteAsync(string routeId);
}
