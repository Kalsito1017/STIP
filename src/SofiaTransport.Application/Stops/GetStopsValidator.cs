using FluentValidation;

namespace SofiaTransport.Application.Stops;

public class GetStopsValidator : AbstractValidator<GetStopsQuery>
{
    public GetStopsValidator()
    {
        // No required fields
    }
}
