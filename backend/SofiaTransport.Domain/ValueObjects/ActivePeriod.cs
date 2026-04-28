namespace SofiaTransport.Domain.ValueObjects;

public sealed record ActivePeriod
{
    public long? Start { get; init; }
    public long? End { get; init; }
}