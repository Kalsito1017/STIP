import { GeoJSON } from 'react-leaflet';
import * as L from 'leaflet';
import type { StopFeatureCollection } from '../../types/map';

interface Props {
  data: StopFeatureCollection;
}

const circleOptions: L.CircleMarkerOptions = {
  radius: 5,
  fillColor: '#ef4444',
  color: '#ffffff',
  weight: 2,
  fillOpacity: 1,
};

export function StopLayer({ data }: Props) {
  return (
    <GeoJSON
      data={data}
      pointToLayer={(_feature, latlng) => L.circleMarker(latlng, circleOptions)}
      key={data.features.length}
    />
  );
}
