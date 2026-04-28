using System.Text.Json;
using NetTopologySuite.Geometries;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;
using Coordinates = SofiaTransport.Domain.ValueObjects.Coordinates;
using StackExchange.Redis;

namespace SofiaTransport.Infrastructure.Cache;

public class RedisVehicleCache : IVehicleCache
{
    private readonly IDatabase _db;
    private const string KeyPrefix = "vehicle:";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public RedisVehicleCache(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

    public async Task<IReadOnlyList<Vehicle>> GetAllAsync()
    {
        var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
        var keys = new List<RedisKey>();
        await foreach (var key in server.KeysAsync(pattern: $"{KeyPrefix}*"))
            keys.Add(key);

        if (keys.Count == 0) return Array.Empty<Vehicle>();

        var values = await _db.StringGetAsync(keys.ToArray());
        return values.Select(v => Deserialize(v!)).Where(v => v is not null).Select(v => v!).ToList();
    }

    public async Task<IReadOnlyList<Vehicle>> GetByRouteAsync(string routeId)
    {
        var all = await GetAllAsync();
        return all.Where(v => v.RouteId == routeId).ToList();
    }

    public async Task<Vehicle?> GetAsync(string vehicleId)
    {
        var value = await _db.StringGetAsync($"{KeyPrefix}{vehicleId}");
        return value.HasValue ? Deserialize(value) : null;
    }

    public Task SetAsync(Vehicle vehicle)
    {
        var json = JsonSerializer.Serialize(new
        {
            vehicleId = vehicle.VehicleId,
            routeId = vehicle.RouteId,
            tripId = vehicle.TripId,
            lat = vehicle.Location.Lat,
            lon = vehicle.Location.Lon,
            vehicle.Bearing,
            vehicle.Speed,
            recordedAt = vehicle.RecordedAt
        }, JsonOptions);
        return _db.StringSetAsync($"{KeyPrefix}{vehicle.VehicleId}", json, TimeSpan.FromSeconds(120));
    }

    public Task RemoveAsync(string vehicleId) =>
        _db.KeyDeleteAsync($"{KeyPrefix}{vehicleId}");

    private static Vehicle? Deserialize(RedisValue value)
    {
        try
        {
            using var doc = JsonDocument.Parse(value.ToString());
            var root = doc.RootElement;
            var lat = root.GetProperty("lat").GetDouble();
            var lon = root.GetProperty("lon").GetDouble();
            return new Vehicle
            {
                VehicleId = root.GetProperty("vehicleId").GetString()!,
                RouteId = root.TryGetProperty("routeId", out var r) && r.ValueKind != JsonValueKind.Null ? r.GetString() : null,
                TripId = root.TryGetProperty("tripId", out var t) && t.ValueKind != JsonValueKind.Null ? t.GetString() : null,
                Location = new Coordinates(lat, lon),
                Geometry = new Point(lon, lat) { SRID = 4326 },
                Bearing = root.TryGetProperty("bearing", out var b) ? b.GetSingle() : 0,
                Speed = root.TryGetProperty("speed", out var s) ? s.GetSingle() : 0,
                RecordedAt = root.GetProperty("recordedAt").GetDateTime()
            };
        }
        catch { return null; }
    }
}
