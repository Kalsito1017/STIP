using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Users;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserDto?>;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);

        return user is null
            ? null
            : new UserDto(user.Id, user.Email, user.FullName, user.CreatedAt);
    }
}
