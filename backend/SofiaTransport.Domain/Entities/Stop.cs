using NetTopologySuite.Geometries;
using SofiaTransport.Domain.ValueObjects;
using Coordinates = SofiaTransport.Domain.ValueObjects.Coordinates;

namespace SofiaTransport.Domain.Entities;

public class Stop
{
    public string StopId { get; set; } = string.Empty;
    public string StopName { get; set; } = string.Empty;
    public Coordinates Location { get; set; } = null!;
    public Point Geometry { get; set; } = null!;
    public double Lat => Geometry.Y;
    public double Lon => Geometry.X;
}
