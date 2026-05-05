using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Analytics;

public record GetDelayHeatmapQuery(DateTime? From = null, DateTime? To = null) : IRequest<IReadOnlyList<HeatmapPointDto>>;

public class GetDelayHeatmapHandler : IRequestHandler<GetDelayHeatmapQuery, IReadOnlyList<HeatmapPointDto>>
{
    private readonly IDelayLogRepository _delayRepo;

    public GetDelayHeatmapHandler(IDelayLogRepository delayRepo)
    {
        _delayRepo = delayRepo;
    }

    public async Task<IReadOnlyList<HeatmapPointDto>> Handle(GetDelayHeatmapQuery request, CancellationToken ct)
    {
        var from = request.From ?? DateTime.UtcNow.AddDays(-7);
        var to = request.To ?? DateTime.UtcNow;

        return await _delayRepo.GetHeatmapAggregatedAsync(from, to);
    }
}
