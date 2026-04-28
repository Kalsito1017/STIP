using Microsoft.EntityFrameworkCore;
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

    private async Task CleanupStaleVehiclesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransportDbContext>();
        var cache = scope.ServiceProvider.GetRequiredService<IVehicleCache>();

        var staleThreshold = DateTime.UtcNow.AddMinutes(-10);
        var staleVehicles = await db.Vehicles.Where(v => v.RecordedAt < staleThreshold).ToListAsync(ct);

        if (staleVehicles.Count > 0)
        {
            db.Vehicles.RemoveRange(staleVehicles);
            await db.SaveChangesAsync(ct);

            foreach (var vehicle in staleVehicles)
            {
                await cache.RemoveAsync(vehicle.VehicleId);
            }

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

    private async Task WriteDelayLogAsync(TransportDbContext db, Vehicle vehicle, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(vehicle.TripId) || string.IsNullOrEmpty(vehicle.RouteId))
            return;

        var stopTime = await FindNearestStopTimeAsync(db, vehicle, ct);
        if (stopTime is null) return;

        if (!await ShouldLogDelayAsync(db, vehicle, stopTime, ct))
            return;

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
        if (string.IsNullOrEmpty(vehicle.TripId))
            return null;

        var now = DateTime.UtcNow.TimeOfDay;

        var stopTimes = await db.StopTimes
            .Where(st => st.TripId == vehicle.TripId)
            .ToListAsync(ct);

        return stopTimes
            .OrderBy(st => Math.Abs((st.ArrivalTime - now).TotalSeconds))
            .FirstOrDefault();
    }

    private static async Task<bool> ShouldLogDelayAsync(TransportDbContext db, Vehicle vehicle, Domain.Entities.StopTime stopTime, CancellationToken ct)
    {
        var recentlyLogged = await db.DelayLogs
            .AnyAsync(d => d.VehicleId == vehicle.VehicleId
                        && d.TripId == vehicle.TripId
                        && d.StopId == stopTime.StopId
                        && d.RecordedAt >= DateTime.UtcNow.AddMinutes(-5), ct);
        return !recentlyLogged;
    }
}
