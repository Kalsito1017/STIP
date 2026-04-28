using Microsoft.EntityFrameworkCore;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Infrastructure.Persistence.Repositories;

public class ReliabilityScoreRepository : IReliabilityScoreRepository
{
    private readonly TransportDbContext _db;

    public ReliabilityScoreRepository(TransportDbContext db) => _db = db;

    public async Task<ReliabilityScore?> GetByIdAsync(object id) => await _db.ReliabilityScores.FindAsync(id);

    public async Task<IReadOnlyList<ReliabilityScore>> GetAllAsync() => await _db.ReliabilityScores.AsNoTracking().ToListAsync();

    public async Task<ReliabilityScore?> GetByRouteAndDateAsync(string routeId, DateTime date) =>
        await _db.ReliabilityScores.FirstOrDefaultAsync(r => r.RouteId == routeId && r.ScoreDate == date.Date);

    public async Task<IReadOnlyList<ReliabilityScore>> GetByRouteAsync(string routeId) =>
        await _db.ReliabilityScores.Where(r => r.RouteId == routeId).OrderByDescending(r => r.ScoreDate)
            .AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<ReliabilityScore>> GetRankingAsync(int top = 10, bool best = true)
    {
        var query = _db.ReliabilityScores
            .GroupBy(r => r.RouteId)
            .Select(g => g.OrderByDescending(r => r.ScoreDate).First());

        return best
            ? await query.OrderByDescending(r => r.Score).Take(top).AsNoTracking().ToListAsync()
            : await query.OrderBy(r => r.Score).Take(top).AsNoTracking().ToListAsync();
    }

    public async Task<ReliabilityScore> AddAsync(ReliabilityScore entity) { _db.ReliabilityScores.Add(entity); await _db.SaveChangesAsync(); return entity; }

    public Task UpdateAsync(ReliabilityScore entity) { _db.ReliabilityScores.Update(entity); return _db.SaveChangesAsync(); }

    public Task DeleteAsync(ReliabilityScore entity) { _db.ReliabilityScores.Remove(entity); return _db.SaveChangesAsync(); }
}
