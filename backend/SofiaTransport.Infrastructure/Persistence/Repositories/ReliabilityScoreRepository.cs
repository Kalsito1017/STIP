using Microsoft.EntityFrameworkCore;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Infrastructure.Persistence.Repositories;

public class ReliabilityScoreRepository : IReliabilityScoreRepository
{
    private readonly TransportDbContext _db;

    public ReliabilityScoreRepository(TransportDbContext db) => _db = db;

    public async Task<ReliabilityScore?> GetByIdAsync(object id) => await Task.FromResult<ReliabilityScore?>(null);

    public async Task<IReadOnlyList<ReliabilityScore>> GetAllAsync() => await _db.ReliabilityScores.AsNoTracking().ToListAsync();

    public async Task<int> GetCountAsync() => await _db.ReliabilityScores.CountAsync();

    public async Task<ReliabilityScore?> GetByRouteAndDateAsync(string routeId, DateTime date) =>
        await _db.ReliabilityScores.FirstOrDefaultAsync(r => r.RouteId == routeId && r.ScoreDate == date.Date);

    public async Task<IReadOnlyList<ReliabilityScore>> GetByRouteAsync(string routeId, DateTime? from = null, DateTime? to = null)
    {
        var query = _db.ReliabilityScores.Where(r => r.RouteId == routeId);
        if (from.HasValue) query = query.Where(r => r.ScoreDate >= from.Value);
        if (to.HasValue) query = query.Where(r => r.ScoreDate <= to.Value);
        return await query.OrderByDescending(r => r.ScoreDate).AsNoTracking().ToListAsync();
    }

    public async Task<ReliabilityScore?> GetLatestByRouteAsync(string routeId) =>
        await _db.ReliabilityScores.Where(r => r.RouteId == routeId)
            .OrderByDescending(r => r.ScoreDate).FirstOrDefaultAsync();

    public async Task<IReadOnlyList<ReliabilityScore>> GetRankingAsync(int top = 10, bool best = true)
    {
        var query = _db.ReliabilityScores
            .GroupBy(r => r.RouteId)
            .Select(g => g.OrderByDescending(r => r.ScoreDate).First());

        return best
            ? await query.OrderByDescending(r => r.Score).Take(top).AsNoTracking().ToListAsync()
            : await query.OrderBy(r => r.Score).Take(top).AsNoTracking().ToListAsync();
    }

    public async Task<ReliabilityScore> AddAsync(ReliabilityScore entity, CancellationToken ct = default) { _db.ReliabilityScores.Add(entity); await _db.SaveChangesAsync(ct); return entity; }

    public async Task UpdateAsync(ReliabilityScore entity, CancellationToken ct = default) { _db.ReliabilityScores.Update(entity); await _db.SaveChangesAsync(ct); }

    public async Task DeleteAsync(ReliabilityScore entity, CancellationToken ct = default) { _db.ReliabilityScores.Remove(entity); await _db.SaveChangesAsync(ct); }
}
