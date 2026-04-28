using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Stops;

public record GetNearbyStopsQuery(
    double Lat,
    double Lon,
    double RadiusKm = 1.0
) : IRequest<IReadOnlyList<StopDto>>;

public class GetNearbyStopsHandler : IRequestHandler<GetNearbyStopsQuery, IReadOnlyList<StopDto>>
{
    private readonly IStopRepository _repo;

    public GetNearbyStopsHandler(IStopRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<StopDto>> Handle(GetNearbyStopsQuery request, CancellationToken ct)
    {
        var stops = await _repo.GetNearbyAsync(request.Lat, request.Lon, request.RadiusKm);
        return stops.Select(s => new StopDto(s.StopId, s.StopName, s.Lat, s.Lon)).ToList();
    }
}
