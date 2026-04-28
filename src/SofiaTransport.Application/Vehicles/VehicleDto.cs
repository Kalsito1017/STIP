namespace SofiaTransport.Application.Vehicles;

public record VehicleDto(
    string VehicleId,
    string? RouteId,
    string? TripId,
    double Lat,
    double Lon,
    float Bearing,
    float Speed,
    DateTime RecordedAt
);
