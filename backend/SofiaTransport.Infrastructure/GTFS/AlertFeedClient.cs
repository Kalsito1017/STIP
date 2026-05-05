using System.Net.Http;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Infrastructure.GTFS;

public class AlertFeedClient : IAlertFeedClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AlertFeedClient> _logger;

    public AlertFeedClient(HttpClient httpClient, ILogger<AlertFeedClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ServiceAlert>> FetchAlertsAsync(CancellationToken ct)
    {
        var response = await _httpClient.GetAsync("", ct);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsByteArrayAsync(ct);
        _logger.LogDebug("Fetched {Bytes} bytes from alerts feed", data.Length);

        var feed = TransitRealtime.FeedMessage.ParseFrom(data);

        if (feed.ParseErrors.Count > 0)
        {
            _logger.LogWarning(
                "Skipped {SkippedCount} malformed entities in alerts feed: {Errors}",
                feed.ParseErrors.Count,
                string.Join("; ", feed.ParseErrors));
        }

        return feed.Entity
            .Where(e => e.Alert is not null)
            .Select(e => ParseAlert(e.Alert!, e.Id))
            .ToList();
    }

    private static ServiceAlert ParseAlert(TransitRealtime.Alert alert, string feedEntityId)
    {
        return new ServiceAlert
        {
            AlertId = feedEntityId,
            HeaderText = alert.HeaderText?.Text ?? string.Empty,
            DescriptionText = alert.DescriptionText?.Text,
            Url = alert.Url?.Text,
            Cause = alert.Cause,
            Effect = alert.Effect,
            Severity = alert.Severity,
            ActivePeriods = alert.ActivePeriods.Select(ap => new ActivePeriod
            {
                Start = ap.Start,
                End = ap.End
            }).ToList(),
            InformedEntities = alert.InformedEntities.Select(ie => new InformedEntity
            {
                AgencyId = ie.AgencyId,
                RouteId = ie.RouteId,
                RouteType = ie.RouteType,
                TripId = ie.Trip?.TripId,
                StopId = ie.StopId
            }).ToList(),
            RecordedAt = DateTime.UtcNow
        };
    }
}