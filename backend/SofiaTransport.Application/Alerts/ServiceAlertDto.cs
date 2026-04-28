using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Application.Alerts;

public record ActivePeriodDto(long? Start, long? End);

public record InformedEntityDto(
    string? AgencyId,
    string? RouteId,
    int? RouteType,
    string? TripId,
    string? StopId
);

public record ServiceAlertDto(
    string AlertId,
    string HeaderText,
    string? DescriptionText,
    string? Url,
    int Cause,
    int Effect,
    int? Severity,
    List<ActivePeriodDto> ActivePeriods,
    List<InformedEntityDto> InformedEntities,
    DateTime RecordedAt
);