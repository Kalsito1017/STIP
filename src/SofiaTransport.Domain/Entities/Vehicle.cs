using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Domain.Entities;

public class Vehicle
{
    public string VehicleId { get; set; } = string.Empty;
    public string? RouteId { get; set; }
    public string? TripId { get; set; }
    public Coordinates Location { get; set; } = null!;
    public float Bearing { get; set; }
    public float Speed { get; set; }
    public DateTime RecordedAt { get; set; }
}
