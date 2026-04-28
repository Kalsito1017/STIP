namespace SofiaTransport.Domain.Entities;

public class Shape
{
    public long Id { get; set; }
    public string RouteId { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public Route Route { get; set; } = null!;
}
