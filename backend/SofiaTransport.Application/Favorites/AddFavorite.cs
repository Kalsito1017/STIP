using MediatR;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Favorites;

public class AddFavoriteHandler : IRequestHandler<AddFavoriteCommand, FavoriteDto>
{
    private readonly IUserFavoriteRepository _repo;

    public AddFavoriteHandler(IUserFavoriteRepository repo) => _repo = repo;

    public async Task<FavoriteDto> Handle(AddFavoriteCommand request, CancellationToken ct)
    {
        if (request.EntityType is not ("route" or "stop"))
            throw new ArgumentException("EntityType must be 'route' or 'stop'.");

        var existing = await _repo.GetByUserAndEntityAsync(request.UserId, request.EntityType, request.EntityId);
        if (existing is not null)
            return new FavoriteDto(existing.Id, existing.EntityType, existing.EntityId, existing.CreatedAt);

        var favorite = new UserFavorite
        {
            UserId = request.UserId,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
        };

        var created = await _repo.AddAsync(favorite, ct);
        return new FavoriteDto(created.Id, created.EntityType, created.EntityId, created.CreatedAt);
    }
}
