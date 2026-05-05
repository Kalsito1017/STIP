using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Analytics;

public record GetReliabilityRankingQuery(int Top = 10, bool Best = true) : IRequest<IReadOnlyList<ReliabilityRankingDto>>;

public class GetReliabilityRankingHandler : IRequestHandler<GetReliabilityRankingQuery, IReadOnlyList<ReliabilityRankingDto>>
{
    private readonly IReliabilityScoreRepository _scoreRepo;
    private readonly IRouteRepository _routeRepo;

    public GetReliabilityRankingHandler(IReliabilityScoreRepository scoreRepo, IRouteRepository routeRepo)
    {
        _scoreRepo = scoreRepo;
        _routeRepo = routeRepo;
    }

    public async Task<IReadOnlyList<ReliabilityRankingDto>> Handle(GetReliabilityRankingQuery request, CancellationToken ct)
    {
        var ranking = await _scoreRepo.GetRankingAsync(request.Top, request.Best);
        var routeIds = ranking.Select(s => s.RouteId).Distinct().ToList();

        var routes = await _routeRepo.GetByIdsAsync(routeIds);
        var routeDict = routes.ToDictionary(r => r.RouteId);

        return ranking
            .Select(s => new ReliabilityRankingDto(
                s.RouteId,
                routeDict.GetValueOrDefault(s.RouteId)?.ShortName ?? s.RouteId,
                s.Score, s.OnTimePct, s.AvgDelaySeconds
            ))
            .ToList();
    }
}
