namespace SofiaTransport.Application.Users;

public record UserDto(Guid Id, string Email, string FullName, DateTime CreatedAt);
public record AuthResponseDto(Guid UserId, string Email, string FullName, string Token);
public record RegisterUserRequest(string Email, string Password, string FullName);
public record LoginUserRequest(string Email, string Password);
