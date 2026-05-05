using Microsoft.EntityFrameworkCore;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Infrastructure.Persistence;

namespace SofiaTransport.Infrastructure.Persistence.Repositories;

public class StopTimeRepository : IStopTimeRepository
{
    private readonly TransportDbContext _db;

    public StopTimeRepository(TransportDbContext db) => _db = db;

    public async Task<IReadOnlyList<StopTime>> GetUpcomingByStopAsync(string stopId, TimeSpan fromTime, int limit)
    {
        return await _db.StopTimes
            .AsNoTracking()
            .Include(st => st.Trip)
            .ThenInclude(t => t.Route)
            .Where(st => st.StopId == stopId && st.ArrivalTime >= fromTime)
            .OrderBy(st => st.ArrivalTime)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<StopTime>> GetByTripAsync(string tripId)
    {
        return await _db.StopTimes
            .AsNoTracking()
            .Where(st => st.TripId == tripId)
            .OrderBy(st => st.StopSequence)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<StopTime>> GetByStopAndRouteAsync(string stopId, string routeId)
    {
        return await _db.StopTimes
            .AsNoTracking()
            .Include(st => st.Trip)
            .Where(st => st.StopId == stopId && st.Trip != null && st.Trip.RouteId == routeId)
            .ToListAsync();
    }
}
