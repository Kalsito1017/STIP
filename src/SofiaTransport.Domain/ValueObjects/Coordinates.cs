namespace SofiaTransport.Domain.ValueObjects;

public sealed record Coordinates
{
    public double Lat { get; }
    public double Lon { get; }

    public Coordinates(double lat, double lon)
    {
        if (lat < 42.5 || lat > 42.85)
            throw new ArgumentOutOfRangeException(nameof(lat), "Latitude must be within Sofia area (42.5 - 42.85)");
        if (lon < 23.1 || lon > 23.6)
            throw new ArgumentOutOfRangeException(nameof(lon), "Longitude must be within Sofia area (23.1 - 23.6)");

        Lat = lat;
        Lon = lon;
    }

    public override string ToString() => $"{Lat:F6},{Lon:F6}";
}
