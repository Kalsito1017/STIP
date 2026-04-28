using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Routes;

public record GetRouteReliabilityHistoryQuery(
    string RouteId,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<IReadOnlyList<ReliabilityHistoryDto>>;

public class GetRouteReliabilityHistoryHandler : IRequestHandler<GetRouteReliabilityHistoryQuery, IReadOnlyList<ReliabilityHistoryDto>>
{
    private readonly IReliabilityScoreRepository _scoreRepo;

    public GetRouteReliabilityHistoryHandler(IReliabilityScoreRepository scoreRepo) => _scoreRepo = scoreRepo;

    public async Task<IReadOnlyList<ReliabilityHistoryDto>> Handle(GetRouteReliabilityHistoryQuery request, CancellationToken ct)
    {
        var scores = await _scoreRepo.GetByRouteAsync(request.RouteId);

        var from = request.From ?? DateTime.UtcNow.AddDays(-30);
        var to = request.To ?? DateTime.UtcNow;

        return scores
            .Where(s => s.ScoreDate >= from && s.ScoreDate <= to)
            .Select(s => new ReliabilityHistoryDto(
                s.ScoreDate,
                s.OnTimePct,
                s.AvgDelaySeconds,
                s.Score,
                s.PeakScore
            ))
            .OrderByDescending(s => s.Date)
            .ToList();
    }
}
