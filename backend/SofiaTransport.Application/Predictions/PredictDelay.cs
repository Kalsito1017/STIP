using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Predictions;

public record PredictDelayCommand(
    string RouteId,
    string StopId,
    int Hour,
    int DayOfWeek,
    int StopSequence
) : IRequest<PredictDelayResponse>;

public class PredictDelayHandler : IRequestHandler<PredictDelayCommand, PredictDelayResponse>
{
    private readonly IMLService _mlService;

    public PredictDelayHandler(IMLService mlService) => _mlService = mlService;

    public async Task<PredictDelayResponse> Handle(PredictDelayCommand request, CancellationToken ct)
    {
        return await _mlService.PredictDelayAsync(request.RouteId, request.StopId,
            request.Hour, request.DayOfWeek, request.StopSequence, ct);
    }
}
