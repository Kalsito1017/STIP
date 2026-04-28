using FluentValidation;

namespace SofiaTransport.Application.Predictions;

public class PredictTravelTimeValidator : AbstractValidator<PredictTravelTimeCommand>
{
    public PredictTravelTimeValidator()
    {
        RuleFor(x => x.FromStopId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.ToStopId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.RouteId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.DepartureTime)
            .GreaterThan(DateTime.UtcNow);
    }
}
