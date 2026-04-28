using FluentValidation;

namespace SofiaTransport.Application.Vehicles;

public class GetLiveVehiclesValidator : AbstractValidator<GetLiveVehiclesQuery>
{
    public GetLiveVehiclesValidator()
    {
        RuleFor(x => x.RouteId)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.RouteId));
    }
}
