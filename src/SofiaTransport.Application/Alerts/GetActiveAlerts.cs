using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Alerts;

public record GetActiveAlertsQuery(string? RouteId = null) : IRequest<IReadOnlyList<ServiceAlertDto>>;

public class GetActiveAlertsHandler : IRequestHandler<GetActiveAlertsQuery, IReadOnlyList<ServiceAlertDto>>
{
    private readonly IAlertCache _cache;

    public GetActiveAlertsHandler(IAlertCache cache) => _cache = cache;

    public async Task<IReadOnlyList<ServiceAlertDto>> Handle(GetActiveAlertsQuery request, CancellationToken ct)
    {
        var alerts = string.IsNullOrEmpty(request.RouteId)
            ? await _cache.GetAllAsync()
            : await _cache.GetByRouteAsync(request.RouteId);

        return alerts.Select(a => new ServiceAlertDto(
            a.AlertId,
            a.HeaderText,
            a.DescriptionText,
            a.Url,
            a.Cause,
            a.Effect,
            a.Severity,
            a.ActivePeriods.Select(ap => new ActivePeriodDto(ap.Start, ap.End)).ToList(),
            a.InformedEntities.Select(ie => new InformedEntityDto(
                ie.AgencyId, ie.RouteId, ie.RouteType, ie.TripId, ie.StopId
            )).ToList(),
            a.RecordedAt
        )).ToList();
    }
}