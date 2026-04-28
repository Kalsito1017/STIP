using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Stops;

public record GetStopCongestionQuery(string StopId, DateTime? Date = null) : IRequest<IReadOnlyList<StopCongestionDto>>;

public class GetStopCongestionHandler : IRequestHandler<GetStopCongestionQuery, IReadOnlyList<StopCongestionDto>>
{
    private readonly IDelayLogRepository _repo;

    public GetStopCongestionHandler(IDelayLogRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<StopCongestionDto>> Handle(GetStopCongestionQuery request, CancellationToken ct)
    {
        var targetDate = request.Date ?? DateTime.UtcNow.Date;
        var logs = await _repo.GetByStopAsync(request.StopId, targetDate, targetDate.AddDays(1));

        return logs
            .GroupBy(l => l.ScheduledArrival.Hour)
            .Select(g => new StopCongestionDto(g.Key, g.Count()))
            .OrderBy(c => c.HourOfDay)
            .ToList();
    }
}
