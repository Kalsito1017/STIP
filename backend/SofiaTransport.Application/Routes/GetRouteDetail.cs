using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Routes;

public record GetRouteDetailQuery(string RouteId) : IRequest<RouteDetailDto?>;

public class GetRouteDetailHandler : IRequestHandler<GetRouteDetailQuery, RouteDetailDto?>
{
    private readonly IRouteRepository _routeRepo;
    private readonly IReliabilityScoreRepository _scoreRepo;

    public GetRouteDetailHandler(IRouteRepository routeRepo, IReliabilityScoreRepository scoreRepo)
    {
        _routeRepo = routeRepo;
        _scoreRepo = scoreRepo;
    }

    public async Task<RouteDetailDto?> Handle(GetRouteDetailQuery request, CancellationToken ct)
    {
        var route = await _routeRepo.GetByIdAsync(request.RouteId);
        if (route is null) return null;

        var scores = await _scoreRepo.GetByRouteAsync(request.RouteId);
        var latest = scores.OrderByDescending(s => s.ScoreDate).FirstOrDefault();

        return new RouteDetailDto(
            route.RouteId, route.ShortName, route.LongName, route.Type,
            latest is not null
                ? new ReliabilityDto(latest.OnTimePct, latest.AvgDelaySeconds, latest.Score, latest.PeakScore, latest.SampleCount)
                : null
        );
    }
}
