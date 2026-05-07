using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IUserFavoriteRepository
{
    Task<IReadOnlyList<UserFavorite>> GetByUserIdAsync(Guid userId);
    Task<UserFavorite?> GetByUserAndEntityAsync(Guid userId, string entityType, string entityId);
    Task<UserFavorite> AddAsync(UserFavorite favorite, CancellationToken ct = default);
    Task<bool> RemoveAsync(long id, Guid userId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid userId, string entityType, string entityId);
}
