using System.Text.Json;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;
using StackExchange.Redis;

namespace SofiaTransport.Infrastructure.Cache;

public class RedisAlertCache : IAlertCache
{
    private readonly IDatabase _db;
    private const string KeyPrefix = "alert:";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public RedisAlertCache(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

    public async Task<IReadOnlyList<ServiceAlert>> GetAllAsync()
    {
        var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
        var keys = server.Keys(pattern: $"{KeyPrefix}*").ToArray();
        if (keys.Length == 0) return Array.Empty<ServiceAlert>();

        var values = await _db.StringGetAsync(keys);
        return values.Select(v => Deserialize(v!)).Where(v => v is not null).Select(v => v!).ToList();
    }

    public async Task<IReadOnlyList<ServiceAlert>> GetByRouteAsync(string routeId)
    {
        var all = await GetAllAsync();
        return all.Where(a => a.InformedEntities.Any(ie => ie.RouteId == routeId)).ToList();
    }

    public Task SetAsync(ServiceAlert alert)
    {
        var json = JsonSerializer.Serialize(new
        {
            alert.AlertId,
            alert.HeaderText,
            alert.DescriptionText,
            alert.Url,
            alert.Cause,
            alert.Effect,
            alert.Severity,
            ActivePeriods = alert.ActivePeriods.Select(ap => new { ap.Start, ap.End }),
            InformedEntities = alert.InformedEntities.Select(ie => new { ie.AgencyId, ie.RouteId, ie.RouteType, ie.TripId, ie.StopId }),
            alert.RecordedAt
        }, JsonOptions);
        return _db.StringSetAsync($"{KeyPrefix}{alert.AlertId}", json, TimeSpan.FromSeconds(300));
    }

    public Task RemoveAsync(string alertId) =>
        _db.KeyDeleteAsync($"{KeyPrefix}{alertId}");

    private static ServiceAlert? Deserialize(RedisValue value)
    {
        try
        {
            using var doc = JsonDocument.Parse(value.ToString());
            var root = doc.RootElement;

            var alert = new ServiceAlert
            {
                AlertId = root.GetProperty("alertId").GetString()!,
                HeaderText = root.GetProperty("headerText").GetString()!,
                DescriptionText = root.TryGetProperty("descriptionText", out var dt) && dt.ValueKind != JsonValueKind.Null ? dt.GetString() : null,
                Url = root.TryGetProperty("url", out var u) && u.ValueKind != JsonValueKind.Null ? u.GetString() : null,
                Cause = root.TryGetProperty("cause", out var c) ? c.GetInt32() : 0,
                Effect = root.TryGetProperty("effect", out var e) ? e.GetInt32() : 0,
                Severity = root.TryGetProperty("severity", out var sev) && sev.ValueKind != JsonValueKind.Null ? sev.GetInt32() : null,
                RecordedAt = root.GetProperty("recordedAt").GetDateTime()
            };

            if (root.TryGetProperty("activePeriods", out var aps) && aps.ValueKind == JsonValueKind.Array)
            {
                foreach (var ap in aps.EnumerateArray())
                {
                    alert.ActivePeriods.Add(new ActivePeriod
                    {
                        Start = ap.TryGetProperty("start", out var s) && s.ValueKind != JsonValueKind.Null ? s.GetInt64() : null,
                        End = ap.TryGetProperty("end", out var end) && end.ValueKind != JsonValueKind.Null ? end.GetInt64() : null
                    });
                }
            }

            if (root.TryGetProperty("informedEntities", out var ies) && ies.ValueKind == JsonValueKind.Array)
            {
                foreach (var ie in ies.EnumerateArray())
                {
                    alert.InformedEntities.Add(new InformedEntity
                    {
                        AgencyId = ie.TryGetProperty("agencyId", out var ai) && ai.ValueKind != JsonValueKind.Null ? ai.GetString() : null,
                        RouteId = ie.TryGetProperty("routeId", out var ri) && ri.ValueKind != JsonValueKind.Null ? ri.GetString() : null,
                        RouteType = ie.TryGetProperty("routeType", out var rt) && rt.ValueKind != JsonValueKind.Null ? rt.GetInt32() : null,
                        TripId = ie.TryGetProperty("tripId", out var ti) && ti.ValueKind != JsonValueKind.Null ? ti.GetString() : null,
                        StopId = ie.TryGetProperty("stopId", out var si) && si.ValueKind != JsonValueKind.Null ? si.GetString() : null
                    });
                }
            }

            return alert;
        }
        catch { return null; }
    }
}