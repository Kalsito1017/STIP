using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Analytics;

public record GetAllStopCongestionQuery(DateTime? Date = null) : IRequest<IReadOnlyList<StopCongestionAllDto>>;

public class GetAllStopCongestionHandler : IRequestHandler<GetAllStopCongestionQuery, IReadOnlyList<StopCongestionAllDto>>
{
    private readonly IDelayLogRepository _delayRepo;
    private readonly IStopRepository _stopRepo;

    public GetAllStopCongestionHandler(IDelayLogRepository delayRepo, IStopRepository stopRepo)
    {
        _delayRepo = delayRepo;
        _stopRepo = stopRepo;
    }

    public async Task<IReadOnlyList<StopCongestionAllDto>> Handle(GetAllStopCongestionQuery request, CancellationToken ct)
    {
        var targetDate = request.Date ?? DateTime.UtcNow.Date;
        var logs = await _delayRepo.GetByDateAsync(targetDate);
        var stops = await _stopRepo.GetAllAsync();

        var stopLookup = stops.ToDictionary(s => s.StopId);

        var congestionByStop = logs
            .Where(l => !string.IsNullOrEmpty(l.StopId))
            .GroupBy(l => l.StopId!)
            .Select(g =>
            {
                var stop = stopLookup.GetValueOrDefault(g.Key);
                var avgDelay = g.Average(l => l.DelaySeconds) ?? 0;
                var sampleCount = g.Count();
                return new StopCongestionAllDto(
                    g.Key,
                    stop?.StopName ?? g.Key,
                    stop?.Lat ?? 0,
                    stop?.Lon ?? 0,
                    avgDelay,
                    sampleCount,
                    GetSeverityLevel(avgDelay)
                );
            })
            .Where(c => stopLookup.ContainsKey(c.StopId))
            .OrderByDescending(c => c.SampleCount)
            .ToList();

        return congestionByStop;
    }

    private static string GetSeverityLevel(double avgDelaySeconds) =>
        avgDelaySeconds <= 30 ? "low" :
        avgDelaySeconds <= 120 ? "medium" :
        avgDelaySeconds <= 300 ? "high" : "severe";
}

public record StopCongestionAllDto(
    string StopId,
    string StopName,
    double Lat,
    double Lon,
    double AvgDelaySeconds,
    int SampleCount,
    string Severity
);
