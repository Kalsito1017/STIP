using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Routes;

public record GetRouteDelayPatternQuery(string RouteId, DateTime? Date = null) : IRequest<IReadOnlyList<DelayPatternDto>>;

public class GetRouteDelayPatternHandler : IRequestHandler<GetRouteDelayPatternQuery, IReadOnlyList<DelayPatternDto>>
{
    private readonly IDelayLogRepository _repo;

    public GetRouteDelayPatternHandler(IDelayLogRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<DelayPatternDto>> Handle(GetRouteDelayPatternQuery request, CancellationToken ct)
    {
        var targetDate = request.Date ?? DateTime.UtcNow.Date;
        var logs = await _repo.GetByRouteAsync(request.RouteId, targetDate, targetDate.AddDays(1));

        return logs
            .GroupBy(l => l.ScheduledArrival.Hour)
            .Select(g => new DelayPatternDto(g.Key, g.Average(l => l.DelaySeconds) ?? 0))
            .OrderBy(d => d.HourOfDay)
            .ToList();
    }
}
