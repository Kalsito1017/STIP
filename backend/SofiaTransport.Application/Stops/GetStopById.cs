using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Stops;

public record GetStopByIdQuery(string StopId) : IRequest<StopDto?>;

public class GetStopByIdHandler : IRequestHandler<GetStopByIdQuery, StopDto?>
{
    private readonly IStopRepository _repo;

    public GetStopByIdHandler(IStopRepository repo) => _repo = repo;

    public async Task<StopDto?> Handle(GetStopByIdQuery request, CancellationToken ct)
    {
        var stop = await _repo.GetByIdAsync(request.StopId);
        if (stop is null) return null;

        return new StopDto(stop.StopId, stop.StopName, stop.Location.Lat, stop.Location.Lon);
    }
}
