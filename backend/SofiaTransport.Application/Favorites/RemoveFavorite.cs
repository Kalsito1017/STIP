using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Favorites;

public class RemoveFavoriteHandler : IRequestHandler<RemoveFavoriteCommand, bool>
{
    private readonly IUserFavoriteRepository _repo;

    public RemoveFavoriteHandler(IUserFavoriteRepository repo) => _repo = repo;

    public async Task<bool> Handle(RemoveFavoriteCommand request, CancellationToken ct)
    {
        return await _repo.RemoveAsync(request.Id, request.UserId, ct);
    }
}
