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
        var vehicles = !string.IsNullOrEmpty(request.RouteId)
            ? await _cache.GetByRouteAsync(request.RouteId)
            : await _cache.GetAllAsync();

        return vehicles
            .Select(v => new VehicleDto(v.VehicleId, v.RouteId, v.TripId, v.Lat, v.Lon, v.Bearing, v.Speed, v.RecordedAt))
            .ToList();
    }
}
