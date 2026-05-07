using Microsoft.EntityFrameworkCore;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Infrastructure.Persistence.Repositories;

public class UserFavoriteRepository : IUserFavoriteRepository
{
    private readonly TransportDbContext _db;

    public UserFavoriteRepository(TransportDbContext db) => _db = db;

    public async Task<IReadOnlyList<UserFavorite>> GetByUserIdAsync(Guid userId) =>
        await _db.UserFavorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task<UserFavorite?> GetByUserAndEntityAsync(Guid userId, string entityType, string entityId) =>
        await _db.UserFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.EntityType == entityType && f.EntityId == entityId);

    public async Task<UserFavorite> AddAsync(UserFavorite favorite, CancellationToken ct = default)
    {
        _db.UserFavorites.Add(favorite);
        await _db.SaveChangesAsync(ct);
        return favorite;
    }

    public async Task<bool> RemoveAsync(long id, Guid userId, CancellationToken ct = default)
    {
        var favorite = await _db.UserFavorites.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId, ct);
        if (favorite is null) return false;
        _db.UserFavorites.Remove(favorite);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid userId, string entityType, string entityId) =>
        await _db.UserFavorites.AnyAsync(f => f.UserId == userId && f.EntityType == entityType && f.EntityId == entityId);
}
