using MediatR;

namespace SofiaTransport.Application.Favorites;

public record FavoriteDto(long Id, string EntityType, string EntityId, DateTime CreatedAt);

public record GetUserFavoritesQuery(Guid UserId) : IRequest<IReadOnlyList<FavoriteDto>>;

public record AddFavoriteCommand(Guid UserId, string EntityType, string EntityId) : IRequest<FavoriteDto>;

public record RemoveFavoriteCommand(Guid UserId, long Id) : IRequest<bool>;
