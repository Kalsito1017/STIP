using FluentValidation;

namespace SofiaTransport.Application.Predictions;

public class PredictDelayValidator : AbstractValidator<PredictDelayCommand>
{
    public PredictDelayValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.StopId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Hour)
            .InclusiveBetween(0, 23);

        RuleFor(x => x.DayOfWeek)
            .InclusiveBetween(0, 6);

        RuleFor(x => x.StopSequence)
            .GreaterThanOrEqualTo(1);
    }
}
