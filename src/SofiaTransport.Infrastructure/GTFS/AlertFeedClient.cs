using System.Net.Http;
using Google.Protobuf;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Infrastructure.GTFS;

public class AlertFeedClient : IAlertFeedClient
{
    private readonly HttpClient _httpClient;

    public AlertFeedClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<IReadOnlyList<ServiceAlert>> FetchAlertsAsync(CancellationToken ct)
    {
        var response = await _httpClient.GetAsync("", ct);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsByteArrayAsync(ct);
        var feed = TransitRealtime.FeedMessage.ParseFrom(data);

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