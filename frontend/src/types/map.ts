export interface RouteShapeFeature {
  type: 'Feature';
  geometry: {
    type: 'LineString';
    coordinates: [number, number][];
  };
  properties: {
    routeId: string;
    shortName: string;
    routeType: string;
    color: string;
    directionId?: number;
  };
}

export interface RouteShapeCollection {
  type: 'FeatureCollection';
  features: RouteShapeFeature[];
}

export interface StopFeature {
  type: 'Feature';
  geometry: {
    type: 'Point';
    coordinates: [number, number];
  };
  properties: {
    stopId: string;
    stopName: string;
  };
}

export interface StopFeatureCollection {
  type: 'FeatureCollection';
  features: StopFeature[];
}
