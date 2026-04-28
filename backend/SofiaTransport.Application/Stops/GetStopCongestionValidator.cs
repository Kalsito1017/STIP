using FluentValidation;

namespace SofiaTransport.Application.Stops;

public class GetStopCongestionValidator : AbstractValidator<GetStopCongestionQuery>
{
    public GetStopCongestionValidator()
    {
        RuleFor(x => x.StopId)
            .NotEmpty()
            .MaximumLength(50);
    }
}
