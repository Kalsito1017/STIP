using FluentValidation;

namespace SofiaTransport.Application.Stops;

public class GetNearbyStopsValidator : AbstractValidator<GetNearbyStopsQuery>
{
    public GetNearbyStopsValidator()
    {
        RuleFor(x => x.Lat)
            .InclusiveBetween(42.5, 42.85)
            .WithMessage("Latitude must be within Sofia area (42.5 - 42.85).");

        RuleFor(x => x.Lon)
            .InclusiveBetween(23.1, 23.6)
            .WithMessage("Longitude must be within Sofia area (23.1 - 23.6).");

        RuleFor(x => x.RadiusKm)
            .InclusiveBetween(0.1, 5.0)
            .WithMessage("Radius must be between 0.1 and 5.0 km.");
    }
}
