namespace SofiaTransport.Application.Analytics;

public record HeatmapPointDto(
    double Lat,
    double Lon,
    double AvgDelaySeconds,
    int SampleCount
);

public record ReliabilityRankingDto(
    string RouteId,
    string ShortName,
    double Score,
    double OnTimePct,
    double AvgDelaySeconds
);

public record PeakHourDto(
    int HourOfDay,
    double AvgDelaySeconds,
    int VehicleCount
);
