using System.Net.Http;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Infrastructure.GTFS;

public class TripUpdateFeedClient : ITripUpdateFeedClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TripUpdateFeedClient> _logger;

    public TripUpdateFeedClient(HttpClient httpClient, ILogger<TripUpdateFeedClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TripUpdate>> FetchTripUpdatesAsync(CancellationToken ct)
    {
        var response = await _httpClient.GetAsync("", ct);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsByteArrayAsync(ct);
        _logger.LogDebug("Fetched {Bytes} bytes from trip-updates feed", data.Length);

        var feed = TransitRealtime.FeedMessage.ParseFrom(data);

        if (feed.ParseErrors.Count > 0)
        {
            _logger.LogWarning(
                "Skipped {SkippedCount} malformed entities in trip-updates feed: {Errors}",
                feed.ParseErrors.Count,
                string.Join("; ", feed.ParseErrors));
        }

        return feed.Entity
            .Where(e => e.TripUpdate is not null)
            .Select(e => ParseTripUpdate(e.TripUpdate!, e.Id))
            .ToList();
    }

    private static TripUpdate ParseTripUpdate(TransitRealtime.TripUpdate tu, string feedEntityId)
    {
        var tripUpdate = new TripUpdate
        {
            TripId = tu.Trip?.TripId ?? string.Empty,
            RouteId = tu.Trip?.RouteId,
            StartTime = tu.Trip?.StartTime,
            StartDate = tu.Trip?.StartDate,
            ScheduleRelationship = tu.Trip?.ScheduleRelationship ?? 0,
            VehicleId = !string.IsNullOrEmpty(tu.Vehicle?.Id) ? tu.Vehicle.Id : null,
            RecordedAt = DateTime.UtcNow
        };

        foreach (var stu in tu.StopTimeUpdates)
        {
            tripUpdate.StopTimeUpdates.Add(new StopTimeUpdate
            {
                StopSequence = stu.StopSequence,
                StopId = stu.StopId,
                ArrivalDelay = stu.Arrival?.Delay,
                ArrivalTime = stu.Arrival?.Time,
                DepartureDelay = stu.Departure?.Delay,
                DepartureTime = stu.Departure?.Time,
                ScheduleRelationship = stu.ScheduleRelationship
            });
        }

        return tripUpdate;
    }
}