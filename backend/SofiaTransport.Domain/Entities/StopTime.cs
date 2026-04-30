namespace SofiaTransport.Domain.Entities;

public class StopTime
{
    public string TripId { get; set; } = string.Empty;
    public string StopId { get; set; } = string.Empty;
    public int StopSequence { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public TimeSpan? DepartureTime { get; set; }
    public Trip Trip { get; set; } = null!;
    public Stop Stop { get; set; } = null!;
}
