namespace SofiaTransport.Domain.Entities;

public class Trip
{
    public string TripId { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public int DirectionId { get; set; }
    public Route Route { get; set; } = null!;
    public ICollection<StopTime> StopTimes { get; set; } = new List<StopTime>();
}
