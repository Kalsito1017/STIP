using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Infrastructure.Cache;
using SofiaTransport.Infrastructure.Persistence;
using SofiaTransport.Infrastructure.Realtime;

namespace SofiaTransport.Worker.Jobs;

public class GtfsPollingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GtfsPollingService> _logger;
    private readonly IConfiguration _config;

    public GtfsPollingService(IServiceScopeFactory scopeFactory, ILogger<GtfsPollingService> logger, IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var interval = _config.GetValue("POLL_INTERVAL_SECONDS", 15);
        _logger.LogInformation("GTFS polling started (interval: {Interval}s)", interval);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await PollAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GTFS poll failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(interval), ct);
        }
    }

    private async Task PollAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var feedClient = scope.ServiceProvider.GetRequiredService<IGtfsFeedClient>();
        var cache = scope.ServiceProvider.GetRequiredService<IVehicleCache>();
        var broadcaster = scope.ServiceProvider.GetRequiredService<IVehicleBroadcaster>();
        var db = scope.ServiceProvider.GetRequiredService<TransportDbContext>();

        var vehicles = await feedClient.FetchVehiclePositionsAsync(ct);
        _logger.LogInformation("Fetched {Count} vehicle positions", vehicles.Count);

        foreach (var vehicle in vehicles)
        {
            vehicle.RecordedAt = DateTime.UtcNow;
            await cache.SetAsync(vehicle);
            await broadcaster.BroadcastAsync(vehicle);

            db.Vehicles.Attach(vehicle);
            db.Entry(vehicle).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            await WriteDelayLogAsync(db, vehicle, ct);
        }

        if (vehicles.Count > 0)
            await db.SaveChangesAsync(ct);
    }

    private async Task WriteDelayLogAsync(TransportDbContext db, Vehicle vehicle, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(vehicle.TripId) || string.IsNullOrEmpty(vehicle.RouteId))
            return;

        var stopTime = await FindNearestStopTimeAsync(db, vehicle, ct);
        if (stopTime is null) return;

        var scheduled = DateTime.UtcNow.Date.Add(stopTime.ArrivalTime);
        var delay = (int)(DateTime.UtcNow - scheduled).TotalSeconds;

        db.DelayLogs.Add(new DelayLog
        {
            VehicleId = vehicle.VehicleId,
            StopId = stopTime.StopId,
            TripId = vehicle.TripId,
            RouteId = vehicle.RouteId,
            ScheduledArrival = scheduled,
            ActualArrival = DateTime.UtcNow,
            DelaySeconds = delay,
            RecordedAt = DateTime.UtcNow
        });
    }

    private static async Task<Domain.Entities.StopTime?> FindNearestStopTimeAsync(TransportDbContext db, Vehicle vehicle, CancellationToken ct)
    {
        return await Task.FromResult<Domain.Entities.StopTime?>(null);
    }
}
