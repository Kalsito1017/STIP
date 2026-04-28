using MediatR;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Application.Shapes;

public record GetAllRouteShapesQuery : IRequest<RouteShapeCollection>;

public class GetAllRouteShapesHandler : IRequestHandler<GetAllRouteShapesQuery, RouteShapeCollection>
{
    private readonly IShapeRepository _shapeRepo;
    private readonly IRouteRepository _routeRepo;

    public GetAllRouteShapesHandler(IShapeRepository shapeRepo, IRouteRepository routeRepo)
    {
        _shapeRepo = shapeRepo;
        _routeRepo = routeRepo;
    }

    public async Task<RouteShapeCollection> Handle(GetAllRouteShapesQuery request, CancellationToken ct)
    {
        var points = await _shapeRepo.GetAllGroupedByRouteAsync();
        var routes = await _routeRepo.GetAllAsync();

        var routeLookup = routes.ToDictionary(r => r.RouteId);
        var features = new List<RouteShapeFeature>();

        var groups = points
            .GroupBy(p => p.RouteId)
            .OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            var route = routeLookup.GetValueOrDefault(group.Key);
            var coordinates = group
                .OrderBy(p => p.Sequence)
                .Select(p => new List<double> { p.Lon, p.Lat })
                .ToList();

            var routeType = route?.Type.ToString() ?? "Bus";
            var color = GetRouteShapeHandler.GetRouteColor(route?.Type);

            features.Add(new RouteShapeFeature(
                "Feature",
                new RouteShapeGeometry("LineString", coordinates),
                new RouteShapeProperties(
                    group.Key,
                    route?.ShortName ?? group.Key,
                    routeType,
                    color
                )
            ));
        }

        return new RouteShapeCollection("FeatureCollection", features);
    }
}
