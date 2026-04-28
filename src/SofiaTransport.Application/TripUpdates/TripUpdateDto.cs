namespace SofiaTransport.Application.TripUpdates;

public record StopTimeUpdateDto(
    int? StopSequence,
    string? StopId,
    int? ArrivalDelay,
    long? ArrivalTime,
    int? DepartureDelay,
    long? DepartureTime,
    int ScheduleRelationship
);

public record TripUpdateDto(
    string TripId,
    string? RouteId,
    string? StartTime,
    string? StartDate,
    int ScheduleRelationship,
    string? VehicleId,
    List<StopTimeUpdateDto> StopTimeUpdates,
    DateTime RecordedAt
);