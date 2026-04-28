using Microsoft.EntityFrameworkCore;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Infrastructure.Persistence.Repositories;

public class ShapeRepository : IShapeRepository
{
    private readonly TransportDbContext _db;

    public ShapeRepository(TransportDbContext db) => _db = db;

    public async Task<IReadOnlyList<Shape>> GetByRouteIdAsync(string routeId) =>
        await _db.Shapes
            .Where(s => s.RouteId == routeId)
            .OrderBy(s => s.Sequence)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<Shape>> GetAllGroupedByRouteAsync() =>
        await _db.Shapes
            .OrderBy(s => s.RouteId)
            .ThenBy(s => s.Sequence)
            .AsNoTracking()
            .ToListAsync();
}
