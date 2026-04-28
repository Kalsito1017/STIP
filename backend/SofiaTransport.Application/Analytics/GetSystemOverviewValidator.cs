using FluentValidation;

namespace SofiaTransport.Application.Analytics;

public class GetSystemOverviewValidator : AbstractValidator<GetSystemOverviewQuery>
{
    public GetSystemOverviewValidator()
    {
        // No fields to validate
    }
}
