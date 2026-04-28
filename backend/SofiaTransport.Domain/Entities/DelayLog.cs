namespace SofiaTransport.Domain.Entities;

public class DelayLog
{
    public long Id { get; set; }
    public string? VehicleId { get; set; }
    public string? StopId { get; set; }
    public string? TripId { get; set; }
    public string? RouteId { get; set; }
    public DateTime ScheduledArrival { get; set; }
    public DateTime ActualArrival { get; set; }
    public int? DelaySeconds { get; set; }
    public DateTime RecordedAt { get; set; }
}
