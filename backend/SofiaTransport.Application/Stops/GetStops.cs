using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Stops;

public record GetStopsQuery : IRequest<IReadOnlyList<StopDto>>;

public class GetStopsHandler : IRequestHandler<GetStopsQuery, IReadOnlyList<StopDto>>
{
    private readonly IStopRepository _repo;

    public GetStopsHandler(IStopRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<StopDto>> Handle(GetStopsQuery request, CancellationToken ct)
    {
        var stops = await _repo.GetAllAsync();
        return stops.Select(s => new StopDto(s.StopId, s.StopName, s.Lat, s.Lon)).ToList();
    }
}
