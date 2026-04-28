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

    public async Task<IReadOnlyList<Stop>> GetNearbyAsync(double lat, double lon, double radiusKm)
    {
        var point = new Point(lon, lat) { SRID = 4326 };
        return await _db.Stops
            .Where(s => EF.Property<Point>(s, "Geometry").Distance(point) <= radiusKm * 1000)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Stop> AddAsync(Stop entity) { _db.Stops.Add(entity); await _db.SaveChangesAsync(); return entity; }

    public Task UpdateAsync(Stop entity) { _db.Stops.Update(entity); return _db.SaveChangesAsync(); }

    public Task DeleteAsync(Stop entity) { _db.Stops.Remove(entity); return _db.SaveChangesAsync(); }
}
