namespace SofiaTransport.Application.Stops;

public record StopDto(
    string StopId,
    string StopName,
    double Lat,
    double Lon
);

public record StopCongestionDto(
    int HourOfDay,
    int VehicleCount
);

public record PredictedArrivalDto(
    string RouteId,
    string RouteShortName,
    string Destination,
    int ScheduledMinutes,
    int? PredictedDelaySeconds,
    string? PredictionConfidence
);
