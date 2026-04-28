using FluentValidation;

namespace SofiaTransport.Application.Stops;

public class GetPredictedArrivalsValidator : AbstractValidator<GetPredictedArrivalsQuery>
{
    public GetPredictedArrivalsValidator()
    {
        RuleFor(x => x.StopId)
            .NotEmpty()
            .MaximumLength(50);
    }
}
