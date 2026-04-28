using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Application.Routes;

public record RouteDto(
    string RouteId,
    string ShortName,
    string? LongName,
    TransitType Type
);

public record RouteDetailDto(
    string RouteId,
    string ShortName,
    string? LongName,
    TransitType Type,
    ReliabilityDto? LatestReliability
);

public record ReliabilityDto(
    double OnTimePct,
    double AvgDelaySeconds,
    double Score,
    double PeakScore,
    int SampleCount
);

public record DelayPatternDto(
    int HourOfDay,
    double AvgDelaySeconds
);
