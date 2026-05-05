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

public record BatchPredictDelayRequest(
    List<PredictDelayRequest> Items
);

public record BatchPredictDelayResponse(
    List<BatchPredictDelayItem> Results
);

public record BatchPredictDelayItem(
    double PredictedDelaySeconds,
    List<double> ConfidenceInterval,
    string ModelVersion,
    PredictDelayRequest Input
);

public record TravelTimePredictionResponse(
    double PredictedTravelTimeSeconds,
    List<double> ConfidenceInterval,
    string ModelVersion
);
