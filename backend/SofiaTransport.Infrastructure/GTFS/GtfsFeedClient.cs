using System.Net.Http;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Polly;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;
using Coordinates = SofiaTransport.Domain.ValueObjects.Coordinates;

namespace SofiaTransport.Infrastructure.GTFS;

public class GtfsFeedClient : IGtfsFeedClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GtfsFeedClient> _logger;

    private const double SofiaMinLat = 42.5;
    private const double SofiaMaxLat = 42.85;
    private const double SofiaMinLon = 23.1;
    private const double SofiaMaxLon = 23.6;
    private const float MaxSpeedMps = 50; // ~180 km/h max plausible transit speed

    public GtfsFeedClient(HttpClient httpClient, ILogger<GtfsFeedClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Vehicle>> FetchVehiclePositionsAsync(CancellationToken ct)
    {
        var response = await _httpClient.GetAsync("", ct);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsByteArrayAsync(ct);
        _logger.LogDebug("Fetched {Bytes} bytes from vehicle-positions feed", data.Length);

        var feed = TransitRealtime.FeedMessage.ParseFrom(data);

        if (feed.ParseErrors.Count > 0)
        {
            _logger.LogWarning(
                "Skipped {SkippedCount} malformed entities in vehicle-positions feed: {Errors}",
                feed.ParseErrors.Count,
                string.Join("; ", feed.ParseErrors));
        }

        var vehicles = new List<Vehicle>();
        foreach (var entity in feed.Entity.Where(e => e.Vehicle is not null))
        {
            try
            {
                var vehicle = ParseVehicle(entity.Vehicle!, entity.Id);
                if (vehicle is not null)
                    vehicles.Add(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Skipping vehicle {EntityId} with invalid data", entity.Id);
            }
        }
        return vehicles;
    }

    private Vehicle? ParseVehicle(TransitRealtime.VehiclePosition vp, string feedEntityId)
    {
        var vehicleId = !string.IsNullOrEmpty(vp.Vehicle?.Id)
            ? vp.Vehicle.Id
            : $"unknown-{feedEntityId}";

        var lat = vp.Position.Latitude;
        var lon = vp.Position.Longitude;

        if (lat < SofiaMinLat || lat > SofiaMaxLat || lon < SofiaMinLon || lon > SofiaMaxLon)
        {
            _logger.LogDebug("Skipping vehicle {VehicleId} with out-of-bounds coordinates ({Lat}, {Lon})", vehicleId, lat, lon);
            return null;
        }

        var speed = vp.Position?.Speed ?? 0;
        if (speed < 0 || speed > MaxSpeedMps)
        {
            _logger.LogDebug("Clamping vehicle {VehicleId} speed from {Speed} to 0", vehicleId, speed);
            speed = (float)Math.Clamp(speed, 0, MaxSpeedMps);
        }

        return new Vehicle
        {
            VehicleId = vehicleId,
            TripId = vp.Trip?.TripId,
            RouteId = vp.Trip?.RouteId,
            Location = new Coordinates(lat, lon),
            Geometry = new Point(lon, lat) { SRID = 4326 },
            Bearing = vp.Position?.Bearing ?? 0,
            Speed = speed,
            RecordedAt = DateTime.UtcNow
        };
    }
}
