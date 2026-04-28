using MediatR;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Application.Routes;

public record GetRoutesQuery(TransitType? Type = null) : IRequest<IReadOnlyList<RouteDto>>;

public class GetRoutesHandler : IRequestHandler<GetRoutesQuery, IReadOnlyList<RouteDto>>
{
    private readonly IRouteRepository _repo;

    public GetRoutesHandler(IRouteRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<RouteDto>> Handle(GetRoutesQuery request, CancellationToken ct)
    {
        var routes = request.Type.HasValue
            ? await _repo.GetByTypeAsync(request.Type.Value)
            : await _repo.GetAllAsync();

        return routes.Select(r => new RouteDto(r.RouteId, r.ShortName, r.LongName, r.Type)).ToList();
    }
}
