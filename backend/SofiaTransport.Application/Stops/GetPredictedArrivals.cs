using MediatR;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Application.Predictions;

namespace SofiaTransport.Application.Stops;

public record GetPredictedArrivalsQuery(string StopId) : IRequest<IReadOnlyList<PredictedArrivalDto>>;

public class GetPredictedArrivalsHandler : IRequestHandler<GetPredictedArrivalsQuery, IReadOnlyList<PredictedArrivalDto>>
{
    private readonly IStopTimeRepository _stopTimeRepo;
    private readonly IMLService _mlService;

    public GetPredictedArrivalsHandler(IStopTimeRepository stopTimeRepo, IMLService mlService)
    {
        _stopTimeRepo = stopTimeRepo;
        _mlService = mlService;
    }

    public async Task<IReadOnlyList<PredictedArrivalDto>> Handle(GetPredictedArrivalsQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var currentTimeOfDay = now.TimeOfDay;
        var currentDayOfWeek = (int)now.DayOfWeek;
        var currentHour = now.Hour;

        var upcomingStopTimes = await _stopTimeRepo.GetUpcomingByStopAsync(request.StopId, currentTimeOfDay, 5);

        var results = new List<PredictedArrivalDto>();

        foreach (var st in upcomingStopTimes)
        {
            var scheduledMinutes = (int)(st.ArrivalTime - currentTimeOfDay).TotalMinutes;
            if (scheduledMinutes < 0) scheduledMinutes += 24 * 60;

            var prediction = await _mlService.PredictDelayAsync(
                st.Trip.RouteId,
                request.StopId,
                currentHour,
                currentDayOfWeek,
                st.StopSequence,
                ct);

            results.Add(new PredictedArrivalDto(
                st.Trip.RouteId,
                st.Trip.Route.ShortName,
                st.Trip.Route.LongName ?? string.Empty,
                scheduledMinutes,
                (int?)prediction.PredictedDelaySeconds,
                prediction.ModelVersion
            ));
        }

        return results;
    }
}
