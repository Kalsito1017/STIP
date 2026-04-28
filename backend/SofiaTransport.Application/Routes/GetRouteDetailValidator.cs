using FluentValidation;

namespace SofiaTransport.Application.Routes;

public class GetRouteDetailValidator : AbstractValidator<GetRouteDetailQuery>
{
    public GetRouteDetailValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty()
            .MaximumLength(50);
    }
}
