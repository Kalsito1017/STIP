using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.TripUpdates;

public record GetLiveTripUpdatesQuery(string? RouteId = null) : IRequest<IReadOnlyList<TripUpdateDto>>;

public class GetLiveTripUpdatesHandler : IRequestHandler<GetLiveTripUpdatesQuery, IReadOnlyList<TripUpdateDto>>
{
    private readonly ITripUpdateCache _cache;

    public GetLiveTripUpdatesHandler(ITripUpdateCache cache) => _cache = cache;

    public async Task<IReadOnlyList<TripUpdateDto>> Handle(GetLiveTripUpdatesQuery request, CancellationToken ct)
    {
        var updates = string.IsNullOrEmpty(request.RouteId)
            ? await _cache.GetAllAsync()
            : await _cache.GetByRouteAsync(request.RouteId);

        return updates.Select(tu => new TripUpdateDto(
            tu.TripId,
            tu.RouteId,
            tu.StartTime,
            tu.StartDate,
            tu.ScheduleRelationship,
            tu.VehicleId,
            tu.StopTimeUpdates.Select(stu => new StopTimeUpdateDto(
                stu.StopSequence,
                stu.StopId,
                stu.ArrivalDelay,
                stu.ArrivalTime,
                stu.DepartureDelay,
                stu.DepartureTime,
                stu.ScheduleRelationship
            )).ToList(),
            tu.RecordedAt
        )).ToList();
    }
}