using SofiaTransport.Application.Predictions;

namespace SofiaTransport.Application.Common.Interfaces;

public interface IMLService
{
    Task<PredictDelayResponse> PredictDelayAsync(string routeId, string stopId,
        int hour, int dayOfWeek, int stopSequence, CancellationToken ct);
}
