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

        var predictionItems = upcomingStopTimes.Select(st =>
        {
            var scheduledMinutes = (int)(st.ArrivalTime - currentTimeOfDay).TotalMinutes;
            if (scheduledMinutes < 0) scheduledMinutes += 24 * 60;

            return (st, scheduledMinutes, req: new PredictDelayRequest(
                st.Trip.RouteId,
                request.StopId,
                currentHour,
                currentDayOfWeek,
                st.StopSequence
            ));
        }).ToList();

        if (predictionItems.Count == 0) return Array.Empty<PredictedArrivalDto>();

        var batchResponse = await _mlService.PredictDelaysBatchAsync(
            new BatchPredictDelayRequest(predictionItems.Select(p => p.req).ToList()), ct);

        var results = new List<PredictedArrivalDto>();
        for (var i = 0; i < predictionItems.Count; i++)
        {
            var (st, scheduledMinutes, _) = predictionItems[i];
            var prediction = i < batchResponse.Results.Count ? batchResponse.Results[i] : null;

            results.Add(new PredictedArrivalDto(
                st.Trip.RouteId,
                st.Trip.Route.ShortName,
                st.Trip.Route.LongName ?? string.Empty,
                scheduledMinutes,
                (int?)prediction?.PredictedDelaySeconds,
                prediction?.ModelVersion
            ));
        }

        return results;
    }
}
