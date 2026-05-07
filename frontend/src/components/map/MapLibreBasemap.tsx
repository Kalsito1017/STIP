import { useEffect, useRef, useState } from 'react';
import { useMap } from 'react-leaflet';
import L from 'leaflet';
import { createMaplibreGLLayer } from '../../lib/maplibre-gl-leaflet';

interface Props {
  styleUrl: string;
}

export function MapLibreBasemap({ styleUrl }: Props) {
  const map = useMap();
  const layerRef = useRef<L.Layer | null>(null);
  const [fallback, setFallback] = useState(false);

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
      setFallback(false);
    } catch (err) {
      console.error('[MapLibre] GL layer failed, falling back to OSM raster:', err);
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
      setFallback(true);
    }

    return () => {
      if (layerRef.current) {
        map.removeLayer(layerRef.current);
        layerRef.current = null;
      }
    };
  }, [map, styleUrl]);

  if (!fallback) return null;

  return (
    <div className="absolute bottom-20 lg:bottom-6 left-1/2 -translate-x-1/2 z-[1000] pointer-events-none">
      <div className="bg-card/90 backdrop-blur-sm border border-border rounded-full px-3 py-1.5 shadow-lg text-xs text-muted-foreground">
        Using OpenStreetMap tiles (vector tiles unavailable)
      </div>
    </div>
  );
}
