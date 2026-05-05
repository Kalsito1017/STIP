using FluentValidation;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Users;

public class LoginUserValidator : AbstractValidator<LoginUserQuery>
{
    public LoginUserValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x)
            .MustAsync(async (query, ct) =>
            {
                var email = query.Email.Trim().ToLowerInvariant();
                var user = await userRepository.GetByEmailAsync(email);
                return user is not null && BCrypt.Net.BCrypt.Verify(query.Password, user.PasswordHash);
            })
            .WithMessage("Invalid email or password.")
            .When(x => !string.IsNullOrEmpty(x.Email) && !string.IsNullOrEmpty(x.Password));
    }
}
