using NetTopologySuite.Geometries;
using SofiaTransport.Domain.ValueObjects;
using Coordinates = SofiaTransport.Domain.ValueObjects.Coordinates;

namespace SofiaTransport.Domain.Entities;

public class Vehicle
{
    public string VehicleId { get; set; } = string.Empty;
    public string? RouteId { get; set; }
    public string? TripId { get; set; }
    public Coordinates Location { get; set; } = null!;
    public Point Geometry { get; set; } = null!;
    public double Lat => Geometry.Y;
    public double Lon => Geometry.X;
    public float Bearing { get; set; }
    public float Speed { get; set; }
    public DateTime RecordedAt { get; set; }
}
