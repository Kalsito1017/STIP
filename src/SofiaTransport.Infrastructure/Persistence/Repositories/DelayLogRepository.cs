using Microsoft.EntityFrameworkCore;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Infrastructure.Persistence.Repositories;

public class DelayLogRepository : IDelayLogRepository
{
    private readonly TransportDbContext _db;

    public DelayLogRepository(TransportDbContext db) => _db = db;

    public async Task<DelayLog?> GetByIdAsync(object id) => await _db.DelayLogs.FindAsync(id);

    public async Task<IReadOnlyList<DelayLog>> GetAllAsync() => await _db.DelayLogs.AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<DelayLog>> GetByRouteAsync(string routeId, DateTime from, DateTime to) =>
        await _db.DelayLogs.Where(d => d.RouteId == routeId && d.RecordedAt >= from && d.RecordedAt < to)
            .AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<DelayLog>> GetByStopAsync(string stopId, DateTime from, DateTime to) =>
        await _db.DelayLogs.Where(d => d.StopId == stopId && d.RecordedAt >= from && d.RecordedAt < to)
            .AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<DelayLog>> GetForHeatmapAsync(DateTime from, DateTime to) =>
        await _db.DelayLogs.Where(d => d.RecordedAt >= from && d.RecordedAt < to)
            .AsNoTracking().ToListAsync();

    public async Task<DelayLog> AddAsync(DelayLog entity) { _db.DelayLogs.Add(entity); await _db.SaveChangesAsync(); return entity; }

    public Task UpdateAsync(DelayLog entity) { _db.DelayLogs.Update(entity); return _db.SaveChangesAsync(); }

    public Task DeleteAsync(DelayLog entity) { _db.DelayLogs.Remove(entity); return _db.SaveChangesAsync(); }
}
