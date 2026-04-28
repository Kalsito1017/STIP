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

    public async Task<int> GetCountAsync() => await _db.Vehicles.CountAsync();

    public async Task<IReadOnlyList<Vehicle>> GetLiveAsync() =>
        await _db.Vehicles.Where(v => v.RecordedAt >= DateTime.UtcNow.AddMinutes(-2)).AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<Vehicle>> GetByRouteAsync(string routeId) =>
        await _db.Vehicles.Where(v => v.RouteId == routeId && v.RecordedAt >= DateTime.UtcNow.AddMinutes(-2))
            .AsNoTracking().ToListAsync();

    public async Task<Vehicle> AddAsync(Vehicle entity, CancellationToken ct = default) { _db.Vehicles.Add(entity); await _db.SaveChangesAsync(ct); return entity; }

    public async Task UpdateAsync(Vehicle entity, CancellationToken ct = default) { _db.Vehicles.Update(entity); await _db.SaveChangesAsync(ct); }

    public async Task DeleteAsync(Vehicle entity, CancellationToken ct = default) { _db.Vehicles.Remove(entity); await _db.SaveChangesAsync(ct); }
}
