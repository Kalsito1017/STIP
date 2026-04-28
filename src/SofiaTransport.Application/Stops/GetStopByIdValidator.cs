using FluentValidation;

namespace SofiaTransport.Application.Stops;

public class GetStopByIdValidator : AbstractValidator<GetStopByIdQuery>
{
    public GetStopByIdValidator()
    {
        RuleFor(x => x.StopId)
            .NotEmpty()
            .MaximumLength(50);
    }
}
