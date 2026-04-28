namespace SofiaTransport.Application.Predictions;

public record PredictDelayRequest(
    string RouteId,
    string StopId,
    int Hour,
    int DayOfWeek,
    int StopSequence
);

public record PredictDelayResponse(
    double PredictedDelaySeconds,
    List<double> ConfidenceInterval,
    string ModelVersion
);

public record TravelTimePredictionResponse(
    double PredictedTravelTimeSeconds,
    List<double> ConfidenceInterval,
    string ModelVersion
);
