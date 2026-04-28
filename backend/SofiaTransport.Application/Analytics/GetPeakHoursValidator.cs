using FluentValidation;

namespace SofiaTransport.Application.Analytics;

public class GetPeakHoursValidator : AbstractValidator<GetPeakHoursQuery>
{
    public GetPeakHoursValidator()
    {
        // Date is optional; no strict validation required
    }
}
