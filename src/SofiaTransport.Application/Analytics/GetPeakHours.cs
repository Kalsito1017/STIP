using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Analytics;

public record GetPeakHoursQuery(DateTime? Date = null) : IRequest<IReadOnlyList<PeakHourDto>>;

public class GetPeakHoursHandler : IRequestHandler<GetPeakHoursQuery, IReadOnlyList<PeakHourDto>>
{
    private readonly IDelayLogRepository _repo;

    public GetPeakHoursHandler(IDelayLogRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<PeakHourDto>> Handle(GetPeakHoursQuery request, CancellationToken ct)
    {
        var date = request.Date ?? DateTime.UtcNow.Date;
        var logs = await _repo.GetByDateAsync(date);

        return logs
            .GroupBy(l => l.ScheduledArrival.Hour)
            .Select(g => new PeakHourDto(g.Key, g.Average(l => l.DelaySeconds) ?? 0, g.Count()))
            .OrderBy(p => p.HourOfDay)
            .ToList();
    }
}
