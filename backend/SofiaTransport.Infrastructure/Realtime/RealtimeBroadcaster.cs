using Microsoft.AspNetCore.SignalR;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Infrastructure.Realtime;

public interface IRealtimeBroadcaster
{
    Task BroadcastTripUpdateAsync(TripUpdate tripUpdate);
    Task BroadcastAlertAsync(ServiceAlert alert);
}

public class RealtimeBroadcaster : IRealtimeBroadcaster
{
    private readonly IHubContext<VehicleHub> _hub;

    public RealtimeBroadcaster(IHubContext<VehicleHub> hub) => _hub = hub;

    public async Task BroadcastTripUpdateAsync(TripUpdate tripUpdate)
    {
        var payload = new
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
        };

        await _hub.Clients.All.SendAsync("TripUpdated", payload);

        if (!string.IsNullOrEmpty(tripUpdate.RouteId))
            await _hub.Clients.Group($"route:{tripUpdate.RouteId}").SendAsync("TripUpdated", payload);
    }

    public async Task BroadcastAlertAsync(ServiceAlert alert)
    {
        var payload = new
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
        };

        await _hub.Clients.All.SendAsync("AlertUpdated", payload);

        foreach (var ie in alert.InformedEntities)
        {
            if (!string.IsNullOrEmpty(ie.RouteId))
                await _hub.Clients.Group($"route:{ie.RouteId}").SendAsync("AlertUpdated", payload);
        }
    }
}