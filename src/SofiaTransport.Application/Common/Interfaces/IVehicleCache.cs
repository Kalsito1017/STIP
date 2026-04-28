using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IVehicleCache
{
    Task<IReadOnlyList<Vehicle>> GetAllAsync();
    Task<Vehicle?> GetAsync(string vehicleId);
    Task SetAsync(Vehicle vehicle);
    Task RemoveAsync(string vehicleId);
}
