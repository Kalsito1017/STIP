using FluentValidation;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Application.Routes;

public class GetRoutesValidator : AbstractValidator<GetRoutesQuery>
{
    public GetRoutesValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue)
            .WithMessage("Type must be a valid transit type.");
    }
}
