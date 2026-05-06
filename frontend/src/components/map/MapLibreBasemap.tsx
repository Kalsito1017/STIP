import { useEffect, useRef } from 'react';
import { useMap } from 'react-leaflet';
import L from 'leaflet';
import { createMaplibreGLLayer } from '../../lib/maplibre-gl-leaflet';

interface Props {
  styleUrl: string;
}

export function MapLibreBasemap({ styleUrl }: Props) {
  const map = useMap();
  const layerRef = useRef<L.Layer | null>(null);

  useEffect(() => {
    if (layerRef.current) {
      map.removeLayer(layerRef.current);
      layerRef.current = null;
    }

    try {
      const glLayer = createMaplibreGLLayer({
        style: styleUrl,
        attribution:
          '© <a href="https://protomaps.com/">Protomaps</a> © <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
      });
      glLayer.addTo(map);
      layerRef.current = glLayer;
    } catch (err) {
      console.error('MapLibre GL layer failed, falling back to raster tiles:', err);
      const raster = (L as any).tileLayer(
        'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
        {
          attribution:
            '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
          maxZoom: 19,
        },
      );
      raster.addTo(map);
      layerRef.current = raster;
    }

    return () => {
      if (layerRef.current) {
        map.removeLayer(layerRef.current);
        layerRef.current = null;
      }
    };
  }, [map, styleUrl]);

  return null;
}
