import { useEffect, useRef, useState, useCallback } from 'react';
import { useMap } from 'react-leaflet';
import L from 'leaflet';
import { createMaplibreGLLayer } from '../../lib/maplibre-gl-leaflet';

interface Props {
  styleUrl: string;
}

const FALLBACK_TIMEOUT_MS = 5000;

export function MapLibreBasemap({ styleUrl }: Props) {
  const map = useMap();
  const layerRef = useRef<L.Layer & { hasLoadFailed?: () => boolean } | null>(null);
  const [fallback, setFallback] = useState(false);
  const fallbackTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const switchToRaster = useCallback(() => {
    if (layerRef.current) {
      try { map.removeLayer(layerRef.current); } catch { /* already removed */ }
      layerRef.current = null;
    }
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
  }, [map]);

  useEffect(() => {
    if (layerRef.current) {
      try { map.removeLayer(layerRef.current); } catch { /* ignore */ }
      layerRef.current = null;
    }
    if (fallbackTimerRef.current) {
      clearTimeout(fallbackTimerRef.current);
      fallbackTimerRef.current = null;
    }
    setFallback(false);

    try {
      const glLayer = createMaplibreGLLayer({
        style: styleUrl,
        attribution:
          '© <a href="https://openfreemap.org/">OpenFreeMap</a> © <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
      });
      glLayer.addTo(map);
      layerRef.current = glLayer;

      fallbackTimerRef.current = setTimeout(() => {
        if (layerRef.current && typeof layerRef.current.hasLoadFailed === 'function' && layerRef.current.hasLoadFailed()) {
          console.warn('[MapLibre] GL layer failed to load tiles, falling back to OSM raster');
          switchToRaster();
        }
      }, FALLBACK_TIMEOUT_MS);
    } catch (err) {
      console.error('[MapLibre] GL layer failed to create, falling back to OSM raster:', err);
      switchToRaster();
    }

    return () => {
      if (fallbackTimerRef.current) {
        clearTimeout(fallbackTimerRef.current);
        fallbackTimerRef.current = null;
      }
      if (layerRef.current) {
        try { map.removeLayer(layerRef.current); } catch { /* ignore */ }
        layerRef.current = null;
      }
    };
  }, [map, styleUrl, switchToRaster]);

  if (!fallback) return null;

  return (
    <div className="absolute bottom-20 lg:bottom-6 left-1/2 -translate-x-1/2 z-[1000] pointer-events-none">
      <div className="bg-card/90 backdrop-blur-sm border border-border rounded-full px-3 py-1.5 shadow-lg text-xs text-muted-foreground">
        Using OpenStreetMap tiles (vector tiles unavailable)
      </div>
    </div>
  );
}
