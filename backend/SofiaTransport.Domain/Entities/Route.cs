using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Domain.Entities;

public class Route
{
    public string RouteId { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string? LongName { get; set; }
    public TransitType Type { get; set; }
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
