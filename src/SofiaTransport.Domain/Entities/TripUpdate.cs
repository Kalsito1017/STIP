namespace SofiaTransport.Domain.Entities;

public class TripUpdate
{
    public string TripId { get; set; } = string.Empty;
    public string? RouteId { get; set; }
    public string? StartTime { get; set; }
    public string? StartDate { get; set; }
    public int ScheduleRelationship { get; set; }
    public string? VehicleId { get; set; }
    public List<StopTimeUpdate> StopTimeUpdates { get; set; } = [];
    public DateTime RecordedAt { get; set; }
}

public class StopTimeUpdate
{
    public int? StopSequence { get; set; }
    public string? StopId { get; set; }
    public int? ArrivalDelay { get; set; }
    public long? ArrivalTime { get; set; }
    public int? DepartureDelay { get; set; }
    public long? DepartureTime { get; set; }
    public int ScheduleRelationship { get; set; }
}