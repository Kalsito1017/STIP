import { GeoJSON } from 'react-leaflet';
import type { RouteShapeCollection } from '../../types/map';

interface Props {
  data: RouteShapeCollection;
}

export function RouteShapeLayer({ data }: Props) {
  return (
    <GeoJSON
      data={data}
      style={(feature) => ({
        color: feature?.properties?.color ?? '#2563eb',
        weight: 4,
        opacity: 0.8,
        lineCap: 'round',
        lineJoin: 'round',
      })}
      key={data.features.length}
    />
  );
}
