using System.Net.Http;
using Google.Protobuf;
using Polly;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Infrastructure.GTFS;

public class GtfsFeedClient : IGtfsFeedClient
{
    private readonly HttpClient _httpClient;

    public GtfsFeedClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<IReadOnlyList<Vehicle>> FetchVehiclePositionsAsync(CancellationToken ct)
    {
        var response = await _httpClient.GetAsync("", ct);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsByteArrayAsync(ct);
        var feed = TransitRealtime.FeedMessage.Parser.ParseFrom(data);

        return feed.Entity
            .Where(e => e.Vehicle is not null)
            .Select(e => ParseVehicle(e.Vehicle!, (uint)e.Id.GetHashCode()))
            .ToList();
    }

    private static Vehicle ParseVehicle(TransitRealtime.VehiclePosition vp, uint feedEntityId)
    {
        var vehicleId = !string.IsNullOrEmpty(vp.Vehicle?.Id)
            ? vp.Vehicle.Id
            : $"unknown-{feedEntityId}";

        return new Vehicle
        {
            VehicleId = vehicleId,
            TripId = vp.Trip?.TripId,
            RouteId = vp.Trip?.RouteId,
            Location = new Coordinates(vp.Position.Latitude, vp.Position.Longitude),
            Bearing = vp.Position?.Bearing ?? 0,
            Speed = vp.Position?.Speed ?? 0,
            RecordedAt = DateTime.UtcNow
        };
    }
}
