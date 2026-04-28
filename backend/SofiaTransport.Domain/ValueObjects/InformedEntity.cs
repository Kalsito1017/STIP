namespace SofiaTransport.Domain.ValueObjects;

public sealed record InformedEntity
{
    public string? AgencyId { get; init; }
    public string? RouteId { get; init; }
    public int? RouteType { get; init; }
    public string? TripId { get; init; }
    public string? StopId { get; init; }
}