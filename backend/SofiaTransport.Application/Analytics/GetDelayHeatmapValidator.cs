using FluentValidation;

namespace SofiaTransport.Application.Analytics;

public class GetDelayHeatmapValidator : AbstractValidator<GetDelayHeatmapQuery>
{
    public GetDelayHeatmapValidator()
    {
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("From date must be before or equal to To date.");
    }
}
