namespace SofiaTransport.Application.Shapes;

public record RouteShapeCollection(
    string Type,
    List<RouteShapeFeature> Features
);

public record RouteShapeFeature(
    string Type,
    RouteShapeGeometry Geometry,
    RouteShapeProperties Properties
);

public record RouteShapeGeometry(
    string Type,
    List<List<double>> Coordinates
);

public record RouteShapeProperties(
    string RouteId,
    string ShortName,
    string RouteType,
    string Color
);
