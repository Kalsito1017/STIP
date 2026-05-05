using Microsoft.EntityFrameworkCore;
using SofiaTransport.Application.Analytics;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Infrastructure.Persistence.Repositories;

public class DelayLogRepository : IDelayLogRepository
{
    private readonly TransportDbContext _db;

    public DelayLogRepository(TransportDbContext db) => _db = db;

    public async Task<DelayLog?> GetByIdAsync(object id) => await _db.DelayLogs.FindAsync(id);

    public async Task<IReadOnlyList<DelayLog>> GetAllAsync() => await _db.DelayLogs.AsNoTracking().ToListAsync();

    public async Task<int> GetCountAsync() => await _db.DelayLogs.CountAsync();

    public async Task<IReadOnlyList<DelayLog>> GetByRouteAsync(string routeId, DateTime from, DateTime to) =>
        await _db.DelayLogs.Where(d => d.RouteId == routeId && d.RecordedAt >= from && d.RecordedAt < to)
            .AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<DelayLog>> GetByStopAsync(string stopId, DateTime from, DateTime to) =>
        await _db.DelayLogs.Where(d => d.StopId == stopId && d.RecordedAt >= from && d.RecordedAt < to)
            .AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<DelayLog>> GetForHeatmapAsync(DateTime from, DateTime to) =>
        await _db.DelayLogs.Where(d => d.RecordedAt >= from && d.RecordedAt < to)
            .AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<HeatmapPointDto>> GetHeatmapAggregatedAsync(DateTime from, DateTime to)
    {
        return await _db.Database.SqlQuery<HeatmapPointDto>(
            $@"SELECT ST_Y(s.location::geometry) AS ""Lat"",
                      ST_X(s.location::geometry) AS ""Lon"",
                      AVG(d.delay_seconds) AS ""AvgDelaySeconds"",
                      COUNT(*)::int AS ""SampleCount""
               FROM delay_logs d
               JOIN stops s ON s.stop_id = d.stop_id
               WHERE d.recorded_at >= {from} AND d.recorded_at < {to}
                 AND d.delay_seconds IS NOT NULL
               GROUP BY s.stop_id, s.location"
        ).ToListAsync();
    }

    public async Task<IReadOnlyList<DelayLog>> GetByDateAsync(DateTime date) =>
        await _db.DelayLogs.Where(d => d.RecordedAt >= date && d.RecordedAt < date.AddDays(1))
            .AsNoTracking().ToListAsync();

    public async Task<DelayLog> AddAsync(DelayLog entity, CancellationToken ct = default) { _db.DelayLogs.Add(entity); await _db.SaveChangesAsync(ct); return entity; }

    public async Task UpdateAsync(DelayLog entity, CancellationToken ct = default) { _db.DelayLogs.Update(entity); await _db.SaveChangesAsync(ct); }

    public async Task DeleteAsync(DelayLog entity, CancellationToken ct = default) { _db.DelayLogs.Remove(entity); await _db.SaveChangesAsync(ct); }
}
