using Microsoft.EntityFrameworkCore;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Infrastructure.Persistence;
using SofiaTransport.Infrastructure.Realtime;

namespace SofiaTransport.Worker.Jobs;

public class GtfsPollingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GtfsPollingService> _logger;
    private readonly IConfiguration _config;
    private readonly string? _tripUpdatesUrl;
    private readonly string? _alertsUrl;
    private int _pollCount = 0;

    public GtfsPollingService(IServiceScopeFactory scopeFactory, ILogger<GtfsPollingService> logger, IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config;
        _tripUpdatesUrl = config["GTFS_RT_TRIP_UPDATES_URL"];
        _alertsUrl = config["GTFS_RT_ALERTS_URL"];
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var interval = _config.GetValue("POLL_INTERVAL_SECONDS", 15);
        _logger.LogInformation("GTFS polling started (interval: {Interval}s)", interval);

        while (!ct.IsCancellationRequested)
        {
            _pollCount++;

            try
            {
                await PollVehiclesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GTFS vehicle poll failed");
            }

            try
            {
                if (!string.IsNullOrEmpty(_tripUpdatesUrl))
                    await PollTripUpdatesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GTFS trip updates poll failed");
            }

            try
            {
                if (!string.IsNullOrEmpty(_alertsUrl))
                    await PollAlertsAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GTFS alerts poll failed");
            }

            if (_pollCount % 10 == 0)
            {
                try
                {
                    await CleanupStaleVehiclesAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Stale vehicle cleanup failed");
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(interval), ct);
        }
    }

    private async Task PollVehiclesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var feedClient = scope.ServiceProvider.GetRequiredService<IGtfsFeedClient>();
        var cache = scope.ServiceProvider.GetRequiredService<IVehicleCache>();
        var broadcaster = scope.ServiceProvider.GetRequiredService<IVehicleBroadcaster>();
        var db = scope.ServiceProvider.GetRequiredService<TransportDbContext>();

        var vehicles = await feedClient.FetchVehiclePositionsAsync(ct);
        vehicles = vehicles.DistinctBy(v => v.VehicleId).ToList();
        _logger.LogInformation("Fetched {Count} vehicle positions", vehicles.Count);

        if (vehicles.Count == 0) return;

        foreach (var v in vehicles) v.RecordedAt = DateTime.UtcNow;

        // Batch load existing vehicles
        var vehicleIds = vehicles.Select(v => v.VehicleId).ToList();
        var existingVehicles = await db.Vehicles
            .Where(v => vehicleIds.Contains(v.VehicleId))
            .AsNoTracking()
            .ToListAsync(ct);
        var existingDict = existingVehicles.ToDictionary(v => v.VehicleId);

        // Batch load stop times for all trip IDs
        var tripIds = vehicles.Where(v => !string.IsNullOrEmpty(v.TripId))
            .Select(v => v.TripId!).Distinct().ToList();
        var stopTimes = tripIds.Count > 0
            ? await db.StopTimes.Where(st => tripIds.Contains(st.TripId)).AsNoTracking().ToListAsync(ct)
            : new List<Domain.Entities.StopTime>();
        var stopTimesByTrip = stopTimes.GroupBy(st => st.TripId).ToDictionary(g => g.Key, g => g.ToList());

        // Batch check recently logged delays
        var recentCheckTasks = vehicles
            .Where(v => !string.IsNullOrEmpty(v.TripId) && !string.IsNullOrEmpty(v.RouteId))
            .Select(async v =>
            {
                var tripStops = stopTimesByTrip.GetValueOrDefault(v.TripId!);
                if (tripStops is null || tripStops.Count == 0) return ((Vehicle v, Domain.Entities.StopTime?, bool))(v, null, false);
                var nearest = FindNearestStopTime(tripStops);
                if (nearest is null) return ((Vehicle v, Domain.Entities.StopTime?, bool))(v, null, false);
                var shouldLog = !await db.DelayLogs.AnyAsync(d =>
                    d.VehicleId == v.VehicleId && d.TripId == v.TripId &&
                    d.StopId == nearest.StopId && d.RecordedAt >= DateTime.UtcNow.AddMinutes(-5), ct);
                return (v, nearest, shouldLog);
            });
        var checkResults = await Task.WhenAll(recentCheckTasks);

        foreach (var (vehicle, stopTime, shouldLog) in checkResults)
        {
            await cache.SetAsync(vehicle);
            await broadcaster.BroadcastAsync(vehicle);

            if (existingDict.TryGetValue(vehicle.VehicleId, out var existing))
            {
                db.Entry(existing).CurrentValues.SetValues(vehicle);
            }
            else
            {
                db.Vehicles.Add(vehicle);
            }

            if (shouldLog && stopTime is not null)
            {
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
        }

        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
    }

    private async Task CleanupStaleVehiclesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransportDbContext>();
        var cache = scope.ServiceProvider.GetRequiredService<IVehicleCache>();

        var staleThreshold = DateTime.UtcNow.AddMinutes(-10);
        var staleVehicles = await db.Vehicles
            .Where(v => v.RecordedAt < staleThreshold)
            .Select(v => v.VehicleId)
            .ToListAsync(ct);

        if (staleVehicles.Count > 0)
        {
            await db.Vehicles.Where(v => staleVehicles.Contains(v.VehicleId)).ExecuteDeleteAsync(ct);

            foreach (var vid in staleVehicles)
                await cache.RemoveAsync(vid);

            _logger.LogInformation("Cleaned up {Count} stale vehicles", staleVehicles.Count);
        }
    }

    private async Task PollTripUpdatesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var feedClient = scope.ServiceProvider.GetRequiredService<ITripUpdateFeedClient>();
        var cache = scope.ServiceProvider.GetRequiredService<ITripUpdateCache>();
        var broadcaster = scope.ServiceProvider.GetRequiredService<IRealtimeBroadcaster>();

        var updates = await feedClient.FetchTripUpdatesAsync(ct);
        _logger.LogInformation("Fetched {Count} trip updates", updates.Count);

        foreach (var tu in updates)
        {
            tu.RecordedAt = DateTime.UtcNow;
            await cache.SetAsync(tu);
            await broadcaster.BroadcastTripUpdateAsync(tu);
        }
    }

    private async Task PollAlertsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var feedClient = scope.ServiceProvider.GetRequiredService<IAlertFeedClient>();
        var cache = scope.ServiceProvider.GetRequiredService<IAlertCache>();
        var broadcaster = scope.ServiceProvider.GetRequiredService<IRealtimeBroadcaster>();

        var alerts = await feedClient.FetchAlertsAsync(ct);
        _logger.LogInformation("Fetched {Count} alerts", alerts.Count);

        foreach (var alert in alerts)
        {
            alert.RecordedAt = DateTime.UtcNow;
            await cache.SetAsync(alert);
            await broadcaster.BroadcastAlertAsync(alert);
        }
    }

    private static Domain.Entities.StopTime? FindNearestStopTime(List<Domain.Entities.StopTime> stopTimes)
    {
        var now = DateTime.UtcNow.TimeOfDay;
        return stopTimes
            .OrderBy(st => Math.Abs((st.ArrivalTime - now).TotalSeconds))
            .FirstOrDefault();
    }
}
