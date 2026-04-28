using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Infrastructure.Persistence.Repositories;

public class RouteRepository : IRouteRepository
{
    private readonly TransportDbContext _db;

    public RouteRepository(TransportDbContext db) => _db = db;

    public async Task<Route?> GetByIdAsync(object id) => await _db.Routes.FindAsync(id);

    public async Task<IReadOnlyList<Route>> GetAllAsync() => await _db.Routes.AsNoTracking().ToListAsync();

    public async Task<int> GetCountAsync() => await _db.Routes.CountAsync();

    public async Task<IReadOnlyList<Route>> GetByTypeAsync(TransitType type) =>
        await _db.Routes.Where(r => r.Type == type).AsNoTracking().ToListAsync();

    public async Task<Route?> GetByShortNameAsync(string shortName) =>
        await _db.Routes.FirstOrDefaultAsync(r => r.ShortName == shortName);

    public async Task<Route> AddAsync(Route entity, CancellationToken ct = default) { _db.Routes.Add(entity); await _db.SaveChangesAsync(ct); return entity; }

    public async Task UpdateAsync(Route entity, CancellationToken ct = default) { _db.Routes.Update(entity); await _db.SaveChangesAsync(ct); }

    public async Task DeleteAsync(Route entity, CancellationToken ct = default) { _db.Routes.Remove(entity); await _db.SaveChangesAsync(ct); }
}
