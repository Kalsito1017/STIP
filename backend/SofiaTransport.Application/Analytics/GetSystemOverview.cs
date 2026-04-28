using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Analytics;

public record GetSystemOverviewQuery : IRequest<SystemOverviewDto>;

public class GetSystemOverviewHandler : IRequestHandler<GetSystemOverviewQuery, SystemOverviewDto>
{
    private readonly IVehicleCache _vehicleCache;
    private readonly IDelayLogRepository _delayRepo;
    private readonly IRouteRepository _routeRepo;
    private readonly IStopRepository _stopRepo;

    public GetSystemOverviewHandler(
        IVehicleCache vehicleCache,
        IDelayLogRepository delayRepo,
        IRouteRepository routeRepo,
        IStopRepository stopRepo)
    {
        _vehicleCache = vehicleCache;
        _delayRepo = delayRepo;
        _routeRepo = routeRepo;
        _stopRepo = stopRepo;
    }

    public async Task<SystemOverviewDto> Handle(GetSystemOverviewQuery request, CancellationToken ct)
    {
        var vehicles = await _vehicleCache.GetAllAsync();
        var totalRoutes = await _routeRepo.GetCountAsync();
        var totalStops = await _stopRepo.GetCountAsync();

        var from = DateTime.UtcNow.AddHours(-1);
        var logs = await _delayRepo.GetForHeatmapAsync(from, DateTime.UtcNow);
        var avgDelay = logs.Any() ? logs.Average(l => l.DelaySeconds) ?? 0 : 0;

        return new SystemOverviewDto(
            vehicles.Count,
            avgDelay,
            totalRoutes,
            totalStops
        );
    }
}
