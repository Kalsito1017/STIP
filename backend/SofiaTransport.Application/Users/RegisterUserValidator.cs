using FluentValidation;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Users;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MustAsync(async (email, ct) =>
            {
                var normalized = email.Trim().ToLowerInvariant();
                var existing = await userRepository.GetByEmailAsync(normalized);
                return existing is null;
            })
            .WithMessage("A user with this email already exists.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.");
    }
}
