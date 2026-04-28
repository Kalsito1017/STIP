using System.Text.Json;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using StackExchange.Redis;

namespace SofiaTransport.Infrastructure.Cache;

public class RedisTripUpdateCache : ITripUpdateCache
{
    private readonly IDatabase _db;
    private const string KeyPrefix = "tripupdate:";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public RedisTripUpdateCache(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

    public async Task<IReadOnlyList<TripUpdate>> GetAllAsync()
    {
        var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
        var keys = server.Keys(pattern: $"{KeyPrefix}*").ToArray();
        if (keys.Length == 0) return Array.Empty<TripUpdate>();

        var values = await _db.StringGetAsync(keys);
        return values.Select(v => Deserialize(v!)).Where(v => v is not null).Select(v => v!).ToList();
    }

    public async Task<IReadOnlyList<TripUpdate>> GetByRouteAsync(string routeId)
    {
        var all = await GetAllAsync();
        return all.Where(tu => tu.RouteId == routeId).ToList();
    }

    public Task SetAsync(TripUpdate tripUpdate)
    {
        var json = JsonSerializer.Serialize(new
        {
            tripUpdate.TripId,
            tripUpdate.RouteId,
            tripUpdate.StartTime,
            tripUpdate.StartDate,
            tripUpdate.ScheduleRelationship,
            tripUpdate.VehicleId,
            StopTimeUpdates = tripUpdate.StopTimeUpdates.Select(stu => new
            {
                stu.StopSequence,
                stu.StopId,
                stu.ArrivalDelay,
                stu.ArrivalTime,
                stu.DepartureDelay,
                stu.DepartureTime,
                stu.ScheduleRelationship
            }),
            tripUpdate.RecordedAt
        }, JsonOptions);
        return _db.StringSetAsync($"{KeyPrefix}{tripUpdate.TripId}", json, TimeSpan.FromSeconds(120));
    }

    public Task RemoveAsync(string tripId) =>
        _db.KeyDeleteAsync($"{KeyPrefix}{tripId}");

    private static TripUpdate? Deserialize(RedisValue value)
    {
        try
        {
            using var doc = JsonDocument.Parse(value.ToString());
            var root = doc.RootElement;

            var tu = new TripUpdate
            {
                TripId = root.GetProperty("tripId").GetString()!,
                RouteId = root.TryGetProperty("routeId", out var r) && r.ValueKind != JsonValueKind.Null ? r.GetString() : null,
                StartTime = root.TryGetProperty("startTime", out var st) && st.ValueKind != JsonValueKind.Null ? st.GetString() : null,
                StartDate = root.TryGetProperty("startDate", out var sd) && sd.ValueKind != JsonValueKind.Null ? sd.GetString() : null,
                ScheduleRelationship = root.TryGetProperty("scheduleRelationship", out var sr) ? sr.GetInt32() : 0,
                VehicleId = root.TryGetProperty("vehicleId", out var vid) && vid.ValueKind != JsonValueKind.Null ? vid.GetString() : null,
                RecordedAt = root.GetProperty("recordedAt").GetDateTime()
            };

            if (root.TryGetProperty("stopTimeUpdates", out var stus) && stus.ValueKind == JsonValueKind.Array)
            {
                foreach (var stu in stus.EnumerateArray())
                {
                    tu.StopTimeUpdates.Add(new StopTimeUpdate
                    {
                        StopSequence = stu.TryGetProperty("stopSequence", out var ss) && ss.ValueKind != JsonValueKind.Null ? ss.GetInt32() : null,
                        StopId = stu.TryGetProperty("stopId", out var si) && si.ValueKind != JsonValueKind.Null ? si.GetString() : null,
                        ArrivalDelay = stu.TryGetProperty("arrivalDelay", out var ad) && ad.ValueKind != JsonValueKind.Null ? ad.GetInt32() : null,
                        ArrivalTime = stu.TryGetProperty("arrivalTime", out var at) && at.ValueKind != JsonValueKind.Null ? at.GetInt64() : null,
                        DepartureDelay = stu.TryGetProperty("departureDelay", out var dd) && dd.ValueKind != JsonValueKind.Null ? dd.GetInt32() : null,
                        DepartureTime = stu.TryGetProperty("departureTime", out var dt) && dt.ValueKind != JsonValueKind.Null ? dt.GetInt64() : null,
                        ScheduleRelationship = stu.TryGetProperty("scheduleRelationship", out var sr2) ? sr2.GetInt32() : 0
                    });
                }
            }

            return tu;
        }
        catch { return null; }
    }
}