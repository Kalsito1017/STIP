using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Domain.Entities;

public class Stop
{
    public string StopId { get; set; } = string.Empty;
    public string StopName { get; set; } = string.Empty;
    public Coordinates Location { get; set; } = null!;
}
