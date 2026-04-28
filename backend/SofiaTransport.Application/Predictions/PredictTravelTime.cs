using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Predictions;

public record PredictTravelTimeCommand(
    string FromStopId,
    string ToStopId,
    string RouteId,
    DateTime DepartureTime
) : IRequest<TravelTimePredictionResponse>;

public class PredictTravelTimeHandler : IRequestHandler<PredictTravelTimeCommand, TravelTimePredictionResponse>
{
    private readonly IStopTimeRepository _stopTimeRepo;
    private readonly IDelayLogRepository _delayLogRepo;

    public PredictTravelTimeHandler(IStopTimeRepository stopTimeRepo, IDelayLogRepository delayLogRepo)
    {
        _stopTimeRepo = stopTimeRepo;
        _delayLogRepo = delayLogRepo;
    }

    public async Task<TravelTimePredictionResponse> Handle(PredictTravelTimeCommand request, CancellationToken ct)
    {
        var fromStopTimes = await _stopTimeRepo.GetByStopAndRouteAsync(request.FromStopId, request.RouteId);
        var toStopTimes = await _stopTimeRepo.GetByStopAndRouteAsync(request.ToStopId, request.RouteId);

        var toTimesByTrip = toStopTimes.ToDictionary(st => st.TripId);

        var travelTimes = new List<double>();
        foreach (var fromSt in fromStopTimes)
        {
            if (toTimesByTrip.TryGetValue(fromSt.TripId, out var toSt) && toSt.StopSequence > fromSt.StopSequence)
            {
                var diff = (toSt.ArrivalTime - fromSt.ArrivalTime).TotalSeconds;
                if (diff > 0)
                    travelTimes.Add(diff);
            }
        }

        var avgScheduledTravelTime = travelTimes.Any() ? travelTimes.Average() : 0;

        var now = DateTime.UtcNow;
        var delayLogs = await _delayLogRepo.GetByRouteAsync(request.RouteId, now.AddDays(-30), now);
        var avgDelay = delayLogs.Any() ? delayLogs.Average(d => d.DelaySeconds) ?? 0 : 0;

        var predictedTravelTimeSeconds = avgScheduledTravelTime + avgDelay;
        var lower = predictedTravelTimeSeconds > 0 ? predictedTravelTimeSeconds * 0.9 : 0;
        var upper = predictedTravelTimeSeconds > 0 ? predictedTravelTimeSeconds * 1.1 : 0;

        return new TravelTimePredictionResponse(
            predictedTravelTimeSeconds,
            new List<double> { lower, upper },
            "heuristic-v1"
        );
    }
}
