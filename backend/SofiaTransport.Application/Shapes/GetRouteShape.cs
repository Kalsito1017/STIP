using MediatR;
using SofiaTransport.Application.Common.Interfaces;
using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Shapes;

public record GetRouteShapeQuery(string RouteId) : IRequest<RouteShapeCollection?>;

public class GetRouteShapeHandler : IRequestHandler<GetRouteShapeQuery, RouteShapeCollection?>
{
    private readonly IShapeRepository _shapeRepo;
    private readonly IRouteRepository _routeRepo;

    public GetRouteShapeHandler(IShapeRepository shapeRepo, IRouteRepository routeRepo)
    {
        _shapeRepo = shapeRepo;
        _routeRepo = routeRepo;
    }

    public async Task<RouteShapeCollection?> Handle(GetRouteShapeQuery request, CancellationToken ct)
    {
        var points = await _shapeRepo.GetByRouteIdAsync(request.RouteId);
        if (points.Count == 0) return null;

        var route = await _routeRepo.GetByIdAsync(request.RouteId);

        var coordinates = points
            .OrderBy(p => p.Sequence)
            .Select(p => new List<double> { p.Lon, p.Lat })
            .ToList();

        var routeType = route?.Type.ToString() ?? "Bus";
        var color = GetRouteColor(route?.Type);

        var feature = new RouteShapeFeature(
            "Feature",
            new RouteShapeGeometry("LineString", coordinates),
            new RouteShapeProperties(
                request.RouteId,
                route?.ShortName ?? request.RouteId,
                routeType,
                color
            )
        );

        return new RouteShapeCollection("FeatureCollection", new List<RouteShapeFeature> { feature });
    }

    public static string GetRouteColor(Domain.Enums.TransitType? type)
    {
        return type switch
        {
            Domain.Enums.TransitType.Bus => "#2563eb",
            Domain.Enums.TransitType.Tram => "#dc2626",
            Domain.Enums.TransitType.Trolley => "#7c3aed",
            Domain.Enums.TransitType.Metro => "#059669",
            _ => "#6b7280"
        };
    }
}
