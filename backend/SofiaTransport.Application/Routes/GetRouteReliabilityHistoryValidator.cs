using FluentValidation;

namespace SofiaTransport.Application.Routes;

public class GetRouteReliabilityHistoryValidator : AbstractValidator<GetRouteReliabilityHistoryQuery>
{
    public GetRouteReliabilityHistoryValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("From date must be before or equal to To date.");
    }
}
