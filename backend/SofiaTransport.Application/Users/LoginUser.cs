using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Users;

public record LoginUserQuery(
    string Email,
    string Password
) : IRequest<AuthResponseDto>;

public class LoginUserHandler : IRequestHandler<LoginUserQuery, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public LoginUserHandler(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = (await _userRepository.GetByEmailAsync(email))!;
        var token = _tokenService.GenerateToken(user);

        return new AuthResponseDto(user.Id, user.Email, user.FullName, token);
    }
}
