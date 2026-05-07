import { GeoJSON } from 'react-leaflet';
import * as L from 'leaflet';
import type { StopFeatureCollection } from '../../types/map';
import i18n from '../../i18n';

interface StopLayerProps {
  data: StopFeatureCollection;
}

const circleOptions: L.CircleMarkerOptions = {
  radius: 6,
  fillColor: '#ef4444',
  color: '#ffffff',
  weight: 2.5,
  fillOpacity: 0.9,
};

export function StopLayer({ data }: StopLayerProps) {
  return (
    <GeoJSON
      data={data}
      pointToLayer={(_feature, latlng) => L.circleMarker(latlng, circleOptions)}
      onEachFeature={(feature, layer) => {
        if (!feature.properties) return;
        const { stopName, stopId } = feature.properties;
        layer.bindTooltip(stopName ?? i18n.t('common:unknown'), {
          direction: 'top',
          offset: [0, -8],
          className: 'rounded-lg px-3 py-1.5 text-sm font-medium shadow-md border-0 bg-card text-foreground',
        });
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
