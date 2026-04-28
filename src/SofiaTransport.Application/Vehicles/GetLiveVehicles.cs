using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Vehicles;

public record GetLiveVehiclesQuery(string? RouteId = null) : IRequest<IReadOnlyList<VehicleDto>>;

public class GetLiveVehiclesHandler : IRequestHandler<GetLiveVehiclesQuery, IReadOnlyList<VehicleDto>>
{
    private readonly IVehicleCache _cache;

    public GetLiveVehiclesHandler(IVehicleCache cache) => _cache = cache;

    public async Task<IReadOnlyList<VehicleDto>> Handle(GetLiveVehiclesQuery request, CancellationToken ct)
    {
        var vehicles = await _cache.GetAllAsync();

        if (!string.IsNullOrEmpty(request.RouteId))
            vehicles = vehicles.Where(v => v.RouteId == request.RouteId).ToList();

        return vehicles
            .Select(v => new VehicleDto(v.VehicleId, v.RouteId, v.TripId, v.Location.Lat, v.Location.Lon, v.Bearing, v.Speed, v.RecordedAt))
            .ToList();
    }
}
