using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Infrastructure.Persistence.Repositories;

public class StopRepository : IStopRepository
{
    private readonly TransportDbContext _db;

    public StopRepository(TransportDbContext db) => _db = db;

    public async Task<Stop?> GetByIdAsync(object id) => await _db.Stops.FindAsync(id);

    public async Task<IReadOnlyList<Stop>> GetAllAsync() => await _db.Stops.AsNoTracking().ToListAsync();

    public async Task<int> GetCountAsync() => await _db.Stops.CountAsync();

    public async Task<IReadOnlyList<Stop>> GetNearbyAsync(double lat, double lon, double radiusKm)
    {
        return await _db.Stops
            .FromSqlRaw(
                @"SELECT * FROM stops WHERE ST_DWithin(location, ST_SetSRID(ST_MakePoint({0}, {1}), 4326)::geography, {2})",
                lon, lat, radiusKm * 1000)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Stop>> GetByIdsAsync(IReadOnlyList<string> stopIds)
    {
        if (stopIds.Count == 0) return Array.Empty<Stop>();
        return await _db.Stops.Where(s => stopIds.Contains(s.StopId)).AsNoTracking().ToListAsync();
    }

    public async Task<Stop> AddAsync(Stop entity, CancellationToken ct = default) { _db.Stops.Add(entity); await _db.SaveChangesAsync(ct); return entity; }

    public async Task UpdateAsync(Stop entity, CancellationToken ct = default) { _db.Stops.Update(entity); await _db.SaveChangesAsync(ct); }

    public async Task DeleteAsync(Stop entity, CancellationToken ct = default) { _db.Stops.Remove(entity); await _db.SaveChangesAsync(ct); }
}
