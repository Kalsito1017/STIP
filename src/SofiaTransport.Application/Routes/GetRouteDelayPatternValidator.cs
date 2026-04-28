using FluentValidation;

namespace SofiaTransport.Application.Routes;

public class GetRouteDelayPatternValidator : AbstractValidator<GetRouteDelayPatternQuery>
{
    public GetRouteDelayPatternValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty()
            .MaximumLength(50);
    }
}
