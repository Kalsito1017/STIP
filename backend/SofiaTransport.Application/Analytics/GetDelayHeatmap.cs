using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Analytics;

public record GetDelayHeatmapQuery(DateTime? From = null, DateTime? To = null) : IRequest<IReadOnlyList<HeatmapPointDto>>;

public class GetDelayHeatmapHandler : IRequestHandler<GetDelayHeatmapQuery, IReadOnlyList<HeatmapPointDto>>
{
    private readonly IDelayLogRepository _delayRepo;
    private readonly IStopRepository _stopRepo;

    public GetDelayHeatmapHandler(IDelayLogRepository delayRepo, IStopRepository stopRepo)
    {
        _delayRepo = delayRepo;
        _stopRepo = stopRepo;
    }

    public async Task<IReadOnlyList<HeatmapPointDto>> Handle(GetDelayHeatmapQuery request, CancellationToken ct)
    {
        var from = request.From ?? DateTime.UtcNow.AddDays(-7);
        var to = request.To ?? DateTime.UtcNow;

        var logs = await _delayRepo.GetForHeatmapAsync(from, to);
        var stops = await _stopRepo.GetAllAsync();
        var stopDict = stops
            .Where(s => s.Geometry is not null)
            .ToDictionary(s => s.StopId);

        return logs
            .Where(l => l.StopId is not null && stopDict.ContainsKey(l.StopId))
            .GroupBy(l => l.StopId!)
            .Select(g =>
            {
                var stop = stopDict[g.Key];
                var delays = g.Where(l => l.DelaySeconds.HasValue).Select(l => (double)l.DelaySeconds!.Value).ToList();
                var avgDelay = delays.Count > 0 ? delays.Average() : 0;
                return new HeatmapPointDto(stop.Lat, stop.Lon, avgDelay, g.Count());
            })
            .ToList();
    }
}
