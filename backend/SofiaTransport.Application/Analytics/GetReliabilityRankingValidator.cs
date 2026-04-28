using FluentValidation;

namespace SofiaTransport.Application.Analytics;

public class GetReliabilityRankingValidator : AbstractValidator<GetReliabilityRankingQuery>
{
    public GetReliabilityRankingValidator()
    {
        RuleFor(x => x.Top)
            .InclusiveBetween(1, 100)
            .WithMessage("Top must be between 1 and 100.");
    }
}
