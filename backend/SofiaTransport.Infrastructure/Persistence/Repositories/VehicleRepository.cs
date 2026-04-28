using Microsoft.EntityFrameworkCore;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Infrastructure.Persistence.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly TransportDbContext _db;

    public VehicleRepository(TransportDbContext db) => _db = db;

    public async Task<Vehicle?> GetByIdAsync(object id) => await _db.Vehicles.FindAsync(id);

    public async Task<IReadOnlyList<Vehicle>> GetAllAsync() => await _db.Vehicles.AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<Vehicle>> GetLiveAsync() =>
        await _db.Vehicles.Where(v => v.RecordedAt >= DateTime.UtcNow.AddMinutes(-2)).AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<Vehicle>> GetByRouteAsync(string routeId) =>
        await _db.Vehicles.Where(v => v.RouteId == routeId && v.RecordedAt >= DateTime.UtcNow.AddMinutes(-2))
            .AsNoTracking().ToListAsync();

    public async Task<Vehicle> AddAsync(Vehicle entity) { _db.Vehicles.Add(entity); await _db.SaveChangesAsync(); return entity; }

    public Task UpdateAsync(Vehicle entity) { _db.Vehicles.Update(entity); return _db.SaveChangesAsync(); }

    public Task DeleteAsync(Vehicle entity) { _db.Vehicles.Remove(entity); return _db.SaveChangesAsync(); }
}
