using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Favorites;

public class GetUserFavoritesHandler : IRequestHandler<GetUserFavoritesQuery, IReadOnlyList<FavoriteDto>>
{
    private readonly IUserFavoriteRepository _repo;

    public GetUserFavoritesHandler(IUserFavoriteRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<FavoriteDto>> Handle(GetUserFavoritesQuery request, CancellationToken ct)
    {
        var favorites = await _repo.GetByUserIdAsync(request.UserId);
        return favorites.Select(f => new FavoriteDto(f.Id, f.EntityType, f.EntityId, f.CreatedAt)).ToList();
    }
}
