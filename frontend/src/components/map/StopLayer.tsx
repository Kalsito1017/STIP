import { GeoJSON } from 'react-leaflet';
import * as L from 'leaflet';
import type { StopFeatureCollection } from '../../types/map';
import i18n from '../../i18n';

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
      onEachFeature={(feature, layer) => {
        if (!feature.properties) return;
        const { stopName, stopId } = feature.properties;
        layer.bindPopup(
          `<div class="text-sm">
            <strong>${stopName ?? i18n.t('common:unknown')}</strong><br/>
            ${i18n.t('common:id_label')} ${stopId ?? 'N/A'}
          </div>`
        );
      }}
      key={JSON.stringify(data.features.length)}
    />
  );
}
